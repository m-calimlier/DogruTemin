using ProcurementData = DogruTemin.Core.Models.ProcurementData;
using ValidationSeverity = DogruTemin.Core.Models.ValidationSeverity;
using OutputDocumentType = DogruTemin.Core.Models.OutputDocumentType;
using DogruTemin.Core.Services;
using DogruTemin.Wpf.Models;
using DogruTemin.Wpf.Services;
using Microsoft.Win32;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace DogruTemin.Wpf.Features.Documents;
internal sealed partial class DocumentsModule
{
    internal void CommissionGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (!Context.State.SuppressDirtyTracking)
            Context.Workspace.MarkDirty();
    }

    internal void CommissionTitle_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!Context.State.SuppressDirtyTracking)
            Context.Workspace.MarkDirty();
    }

    internal void AnyGlobalField_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!Context.State.SuppressDirtyTracking)
            Context.Workspace.MarkDirty();
    }

    internal void AnyDate_Changed(object? sender, RoutedPropertyChangedEventArgs<DateTime?> e)
    {
        UpdateApprovalDateReference();
        if (Context.State.SuppressDirtyTracking)
            return;
        Context.Workspace.MarkDirty();
        var warnings = ValidationService.DateLogicWarnings(Context.Workspace.BuildDataFromUi()).Select(x => x.Message).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
        var warningText = string.Join("\n", warnings);
        if (warningText.Length == 0)
        {
            _lastDateSequenceWarning = string.Empty;
            return;
        }

        if (string.Equals(_lastDateSequenceWarning, warningText, StringComparison.Ordinal))
            return;
        _lastDateSequenceWarning = warningText;
        MessageBox.Show(warningText, "DoğruTemin - Tarih Kontrolü", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    internal void UpdateApprovalDateReference()
    {
        if (View<System.Windows.Controls.TextBlock>("ApprovalDateReferenceText")is null || View<DogruTemin.Wpf.LargeDatePicker>("ApprovalDocumentDatePicker")is null)
            return;
        View<System.Windows.Controls.TextBlock>("ApprovalDateReferenceText").Text = View<DogruTemin.Wpf.LargeDatePicker>("ApprovalDocumentDatePicker").SelectedDate is DateTime date ? $"Onay Belgesi tarihi: {date:dd.MM.yyyy}" : "Onay Belgesi tarihi: Henüz girilmedi";
    }

    internal static IReadOnlyList<DogruTemin.Core.Models.ValidationIssue> GetDateSaveBlockingIssues(DogruTemin.Core.Models.ProcessDates dates) => DateValidationService.RequiredSequenceIssues(dates, ValidationSeverity.Critical);
    internal void SaveDates_Click(object sender, RoutedEventArgs e)
    {
        var issues = GetDateSaveBlockingIssues(Context.Workspace.BuildDataFromUi().Dates);
        if (issues.Count > 0)
        {
            MessageBox.Show(string.Join(Environment.NewLine, issues.Select(issue => issue.Message)), "DoğruTemin - Tarih Sıralaması", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Context.Workspace.Save_Click(sender, e);
    }

    internal void AutoBackup_Changed(object sender, RoutedEventArgs e)
    {
        if (!Context.State.SuppressDirtyTracking)
            Context.Workspace.MarkDirty();
    }

    internal static bool ConfirmNonBlockingDocumentIssues(IReadOnlyList<string> messages, string actionName)
    {
        var distinctMessages = messages.Where(message => !string.IsNullOrWhiteSpace(message)).Distinct().Take(14).ToList();
        if (distinctMessages.Count == 0)
            return true;
        var body = "Belgede eksik veya kontrol edilmesi gereken bilgiler var.\n" + "Bunlar önizlemeyi engellemez; temel çıktı koşulları sağlandığı için " + actionName + " işlemine devam edebilirsiniz.\n\n" + string.Join("\n", distinctMessages.Select(message => "• " + message)) + "\n\nYine de devam etmek istiyor musunuz?";
        return MessageBox.Show(body, "DoğruTemin - Eksik Bilgi Uyarısı", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
    }

    private ProcurementData? _preparedExportData;
    internal FixedDocument? BuildOfficialDocumentForExport(IReadOnlyCollection<OutputDocumentType> types, string actionName)
    {
        _preparedExportData = null;
        if (Context.Workspace.HasUnsavedWork() || string.IsNullOrWhiteSpace(Context.Session.RecordId))
        {
            if (Context.Session.OfferEditorDirty && !Context.Offers.SaveOfferEditor(true))
                return null;
            Context.Workspace.Save_Click(Context.Window, new RoutedEventArgs());
            if (Context.Workspace.HasUnsavedWork() || string.IsNullOrWhiteSpace(Context.Session.RecordId))
                return null;
        }

        var data = Context.Storage.LoadRecord(Context.Session.RecordId, out var warning) ?? throw new InvalidOperationException(warning);
        var result = DocumentWorkflow.Prepare(data, types, DocumentPreparationMode.Export);
        if (result.Status == DocumentPreparationStatus.Blocked)
        {
            MessageBox.Show("Seçili belge grubu için temel çıktı koşulları tamamlanmalıdır.\n\n1. grup (Ön Hazırlık): Kurum bilgisi + her iki komisyon + ürün(ler) ve miktar/adet.\n2. grup (Nihai İşlem): En az 1 ürün + en az 3 firma + yaklaşık/nihai teklifler + her iki komisyon.\n\nEksik olanlar:\n" + string.Join("\n", result.CoreIssues.Take(18).Select(i => "• " + i)), "DoğruTemin - Temel Çıktı Koşulları", MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }

        if (result.Status == DocumentPreparationStatus.ConfirmationRequired)
        {
            if (!ConfirmNonBlockingDocumentIssues(result.ConfirmationMessages, actionName))
                return null;
            result = DocumentWorkflow.Prepare(data, types, DocumentPreparationMode.Export, advisoriesConfirmed: true);
        }

        if (result.Status == DocumentPreparationStatus.WinnerSelectionRequired)
        {
            var tiedFirms = Context.State.Firms.Where(f => result.TiedWinnerFirmIds.Contains(f.Id)).ToList();
            var chooser = new TieWinnerSelectionWindow(tiedFirms, NumberParser.Money(result.TiedMarketTotal ?? 0m))
            {
                Owner = Context.Window
            };
            if (chooser.ShowDialog() != true || string.IsNullOrWhiteSpace(chooser.SelectedFirmId))
                return null;
            Context.Session.PreferredWinnerFirmId = chooser.SelectedFirmId;
            Context.Workspace.MarkDirty();
            Context.Workspace.Save_Click(Context.Window, new RoutedEventArgs());
            if (Context.Workspace.HasUnsavedWork())
                return null;
            data = Context.Storage.LoadRecord(Context.Session.RecordId, out warning) ?? throw new InvalidOperationException(warning);
            result = DocumentWorkflow.Prepare(data, types, DocumentPreparationMode.Export, advisoriesConfirmed: true);
        }

        var document = GetDocumentOrThrow(result);
        if (document is not null)
        {
            PdfVectorWriter.Validate(document);
            _preparedExportData = data;
        }

        return document;
    }

    internal static FixedDocument? GetDocumentOrThrow(DocumentWorkflowResult result)
    {
        if (result.Status == DocumentPreparationStatus.Failed)
            throw result.Error ?? new InvalidOperationException("Belge hazırlığı başarısız oldu.");
        return result.Status == DocumentPreparationStatus.Ready ? result.Document : null;
    }

    internal void OutputGroupPdf_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;
        var preparation = button.Tag as string == "Preparation";
        var types = preparation ? SavedDocumentService.PreparationDocumentTypes : SavedDocumentService.FinalDocumentTypes;
        button.IsEnabled = false;
        try
        {
            if (BuildOfficialDocumentForExport(types, "PDF kaydetme")is null)
                return;
            var data = _preparedExportData ?? throw new InvalidOperationException("Kayıtlı belge kaynağı bulunamadı.");
            var folder = new SavedDocumentService(Context.Storage).ExportSeparate(data, types);
            Context.Session.LastOutput = data.LastOutput;
            var status = preparation ? View<System.Windows.Controls.TextBlock>("PreparationExportStatusText") : View<System.Windows.Controls.TextBlock>("FinalExportStatusText");
            status.Text = $"{types.Count} ayrı PDF oluşturuldu • {DateTime.Now:dd.MM.yyyy HH:mm}";
            Context.Archive.RefreshArchiveRecords();
            Context.Workspace.UpdateHomeActivitySummary();
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(folder) { UseShellExecute = true });
        }
        catch (Exception error)
        {
            MessageBox.Show("PDF dosyaları oluşturulamadı: " + error.Message, "DoğruTemin", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            button.IsEnabled = true;
        }
    }

    internal void ApprovalNoBox_LostFocus(object sender, RoutedEventArgs e)
    {
        var raw = View<System.Windows.Controls.TextBox>("ApprovalNoBox").Text.Trim();
        if (raw.Length == 0)
            return;
        raw = Regex.Replace(raw, @"(?i)\bHYS\b\s*[-:/]*\s*", "").Trim();
        if (View<System.Windows.Controls.TextBox>("ApprovalNoBox").Text != raw)
        {
            Context.State.SuppressDirtyTracking = true;
            View<System.Windows.Controls.TextBox>("ApprovalNoBox").Text = raw;
            Context.State.SuppressDirtyTracking = false;
            Context.Workspace.MarkDirty();
        }
    }
}
