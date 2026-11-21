using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows.Documents;
using DogruTemin.Core.Models;
using DogruTemin.Core.Services;

namespace DogruTemin.Wpf.Services;
public sealed class SavedDocumentService(StorageService storage)
{
    public static IReadOnlyList<OutputDocumentType> PreparationDocumentTypes { get; } = Array.AsReadOnly(new[] { OutputDocumentType.ApproximateRequest, OutputDocumentType.FirmOfferForm });
    public static IReadOnlyList<OutputDocumentType> FinalDocumentTypes { get; } = Array.AsReadOnly(new[] { OutputDocumentType.ApproximateCostSheet, OutputDocumentType.ApprovalDocument, OutputDocumentType.MarketResearchReport, OutputDocumentType.InspectionAcceptanceReport });

    public string ExportSeparate(ProcurementData source, IReadOnlyCollection<OutputDocumentType> types)
    {
        var preparation = types.SequenceEqual(PreparationDocumentTypes);
        if (!preparation && !types.SequenceEqual(FinalDocumentTypes))
            throw new ArgumentException("PDF grubu ön hazırlık veya nihai işlem belgelerinin tamamını içermelidir.", nameof(types));
        var folder = Path.Combine(storage.OutputFolderFor(source.RecordId), preparation ? "On_Hazirlik" : "Nihai_Islem");
        var documents = types.ToDictionary(FileNameFor, type => OfficialDocumentService.BuildDocument(source, new[] { type }));
        return Path.GetDirectoryName(ExportPackage(source, documents, types, folder))!;
    }

    public static string FileNameFor(OutputDocumentType type) => type switch
    {
        OutputDocumentType.ApproximateRequest => "01_Yaklasik_Maliyet_Talep_Yazisi.pdf",
        OutputDocumentType.FirmOfferForm => "02_Firma_Teklif_Formu.pdf",
        OutputDocumentType.ApproximateCostSheet => "03_Yaklasik_Maliyet_Hesap_Cetveli.pdf",
        OutputDocumentType.ApprovalDocument => "04_Onay_Belgesi.pdf",
        OutputDocumentType.MarketResearchReport => "05_Piyasa_Fiyat_Arastirmasi_Tutanagi.pdf",
        OutputDocumentType.InspectionAcceptanceReport => "06_Muayene_ve_Kabul_Tutanagi.pdf",
        _ => throw new ArgumentOutOfRangeException(nameof(type))};
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };
    public string Export(ProcurementData source, FixedDocument document, IReadOnlyCollection<OutputDocumentType> types) => ExportPackage(source, new Dictionary<string, FixedDocument> { ["DogruTemin_Belgeler.pdf"] = document }, types, storage.OutputFolderFor(source.RecordId));
    private string ExportPackage(ProcurementData source, IReadOnlyDictionary<string, FixedDocument> documents, IReadOnlyCollection<OutputDocumentType> types, string folder)
    {
        var current = storage.LoadRecord(source.RecordId, out var warning) ?? throw new InvalidDataException(warning ?? "Önce işlemi kaydediniz.");
        var hash = RecordIntegrity.ContentHash(source);
        if (current.Revision != source.Revision || RecordIntegrity.ContentHash(current) != hash)
            throw new InvalidOperationException("Ekran ile kayıtlı işlem farklı. Önce değişiklikleri kaydediniz.");
        var staging = folder + ".new_" + Guid.NewGuid().ToString("N");
        var oldFolder = folder + ".old_" + Guid.NewGuid().ToString("N");
        Directory.CreateDirectory(staging);
        var primary = documents.Keys.First();
        var path = Path.Combine(folder, primary);
        var stagedPdf = Path.Combine(staging, primary);
        var published = false;
        var committed = false;
        try
        {
            foreach (var(name, document)in documents)
                PdfVectorWriter.Save(document, Path.Combine(staging, name));
            var output = new SavedOutputInfo
            {
                RecordRevision = source.Revision,
                ContentHash = hash,
                PdfHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(stagedPdf))),
                PdfPath = path,
                GeneratedAt = DateTime.Now,
                ApplicationVersion = AppVersionInfo.DisplayVersion,
                DocumentTypes = types.ToList(),
                PdfFileHashes = documents.Keys.ToDictionary(name => name, name => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(staging, name)))))
            };
            // The saved archive is the authoritative input snapshot; this portable copy travels with the PDF.
            var snapshotData = JsonSerializer.Deserialize<ProcurementData>(JsonSerializer.Serialize(source, JsonOptions), JsonOptions)!;
            snapshotData.LastOutput = null;
            var snapshot = JsonSerializer.Serialize(new { Output = output, Data = snapshotData }, JsonOptions);
            var snapshotPath = Path.Combine(staging, "Belge_Kaynak.json");
            File.WriteAllText(snapshotPath, snapshot);
            output.SnapshotHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(snapshotPath)));
            storage.RegisterOutput(source, output, () =>
            {
                if (Directory.Exists(folder))
                    Directory.Move(folder, oldFolder);
                try
                {
                    Directory.Move(staging, folder);
                    published = true;
                }
                catch
                {
                    if (Directory.Exists(oldFolder))
                        Directory.Move(oldFolder, folder);
                    throw;
                }
            });
            committed = true;
            return path;
        }
        catch
        {
            if (published)
            {
                Directory.Delete(folder, true);
                if (Directory.Exists(oldFolder))
                    Directory.Move(oldFolder, folder);
            }

            throw;
        }
        finally
        {
            if (Directory.Exists(staging))
                Directory.Delete(staging, true);
            if (committed && Directory.Exists(oldFolder))
            {
                try
                {
                    Directory.Delete(oldFolder, true);
                }
                catch (IOException)
                { /* Keep the previous package if a viewer still holds it. */
                }
                catch (UnauthorizedAccessException)
                { /* Cleanup must not invalidate a published package. */
                }
            }
        }
    }

    public static string OutputStatus(ProcurementData data)
    {
        if (data.LastOutput is null)
            return "Henüz üretilmedi";
        if (!RecordIntegrity.HasCurrentOutput(data))
            return "Değişti • yeniden üretin";
        if (!File.Exists(data.LastOutput.PdfPath))
            return "PDF bulunamadı";
        try
        {
            var snapshotPath = Path.Combine(Path.GetDirectoryName(data.LastOutput.PdfPath)!, "Belge_Kaynak.json");
            if (!File.Exists(snapshotPath))
                return "Kaynak bilgisi bulunamadı";
            if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(snapshotPath))) != data.LastOutput.SnapshotHash)
                return "Kaynak değişmiş • yeniden üretin";
            foreach (var(fileName, hash)in data.LastOutput.PdfFileHashes)
            {
                if (Path.GetFileName(fileName) != fileName)
                    return "PDF kaynak bilgisi geçersiz";
                var path = Path.Combine(Path.GetDirectoryName(data.LastOutput.PdfPath)!, fileName);
                if (!File.Exists(path))
                    return "PDF bulunamadı";
                if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))) != hash)
                    return "PDF değişmiş • yeniden üretin";
            }

            return Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(data.LastOutput.PdfPath))) == data.LastOutput.PdfHash ? "Güncel PDF ✓" : "PDF değişmiş • yeniden üretin";
        }
        catch (IOException)
        {
            return "PDF okunamıyor";
        }
        catch (UnauthorizedAccessException)
        {
            return "PDF erişimi yok";
        }
    }
}
