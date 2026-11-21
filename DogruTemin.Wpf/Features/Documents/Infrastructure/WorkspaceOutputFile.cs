using System.Globalization;
using System.IO;
using DesktopWorkspace;
using DogruTemin.Core.Services;

namespace DogruTemin.Wpf;
/// <summary>Automatic export destinations; never displays a save-location dialog.</summary>
internal static class WorkspaceOutputFile
{
    internal static string CreatePath(StorageService storage, string fileName, string defaultExtension)
    {
        var name = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Dosya adı boş olamaz.", nameof(fileName));
        if (!Path.HasExtension(name))
            name += defaultExtension;
        var extension = Path.GetExtension(name).ToLowerInvariant();
        var now = DateTime.Now;
        var date = now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var category = extension switch
        {
            ".zip" => Path.Combine(storage.BackupFolder, "Arşiv", date),
            ".json" => Path.Combine(storage.DataFolder, "Dışa Aktarılanlar", date),
            _ => Path.Combine(storage.PdfFolder, date, "Raporlar")};
        var unique = now.ToString("HH-mm-ss-fff", CultureInfo.InvariantCulture) + "_" + Guid.NewGuid().ToString("N")[..8];
        var path = Path.Combine(category, Path.GetFileNameWithoutExtension(name) + "_" + unique + extension);
        if (!WorkspaceOutputPath.IsAllowed(storage.RootFolder, path))
            throw new IOException("Dosya programın masaüstü klasörünün dışına kaydedilemez.");
        Directory.CreateDirectory(category);
        return path;
    }
}
