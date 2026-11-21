using DogruTemin.Core.Modules.Archive;
using DogruTemin.Wpf.Models;
using DogruTemin.Core.Services;
using DogruTemin.Wpf.Services;
using Microsoft.Win32;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DogruTemin.Wpf.Features.Archive;
internal sealed partial class ArchiveModule
{
    internal void RefreshArchiveRecords()
    {
        Context.State.ArchiveRecords.Clear();
        var listing = Context.ArchiveQuery.Read(Context.Session.RecordId);
        foreach (var summary in listing.Records)
        {
            var data = summary.Record;
            var lastSaved = summary.LastSavedAt;
            var isActive = summary.IsActive;
            Context.State.ArchiveRecords.Add(new ArchiveRecordRow { RecordId = data.RecordId, Status = isActive ? "AKTİF" : "ARŞİV", IsActive = isActive, OperationNumber = string.IsNullOrWhiteSpace(data.OperationNumber) ? "—" : data.OperationNumber, JobTitle = string.IsNullOrWhiteSpace(data.JobTitle) ? "Başlıksız işlem" : data.JobTitle, ApprovalDate = data.Dates.ApprovalDate?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) ?? "—", ApprovalSortKey = data.Dates.ApprovalDate ?? DateTime.MinValue, ProductCount = summary.ProductCount, FirmCount = summary.FirmCount, FinalTotal = NumberParser.Money(summary.FinalTotal), FinalTotalValue = summary.FinalTotal, OutputStatus = SavedDocumentService.OutputStatus(data), LastSaved = lastSaved.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture), LastSavedValue = lastSaved });
        }

        ApplyArchiveView();
        View<System.Windows.Controls.TextBlock>("ArchiveWarningsText").Text = string.Join("\n", listing.Warnings);
        View<System.Windows.Controls.TextBlock>("ArchiveWarningsText").Visibility = listing.Warnings.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        Context.Workspace.UpdateHomeActivitySummary();
    }

    internal bool ArchiveRecordMatchesFilter(object item)
    {
        if (item is not ArchiveRecordRow row)
            return false;
        var filterIndex = View<System.Windows.Controls.ComboBox>("ArchiveFilterBox")?.SelectedIndex ?? 0;
        if (filterIndex == 1 && !row.IsActive)
            return false;
        if (filterIndex == 2 && row.IsActive)
            return false;
        var query = View<System.Windows.Controls.TextBox>("ArchiveSearchBox")?.Text?.Trim();
        if (string.IsNullOrWhiteSpace(query))
            return true;
        var searchable = $"{row.OperationNumber} {row.JobTitle} {row.ApprovalDate} {row.LastSaved} {row.Status} {row.OutputStatus}";
        return CultureInfo.GetCultureInfo("tr-TR").CompareInfo.IndexOf(searchable, query, CompareOptions.IgnoreCase) >= 0;
    }

    internal void ApplyArchiveView()
    {
        if (_archiveView is null)
            return;
        _archiveView.Refresh();
        _archiveView.SortDescriptions.Clear();
        switch (View<System.Windows.Controls.ComboBox>("ArchiveSortBox")?.SelectedIndex ?? 0)
        {
            case 1:
                _archiveView.SortDescriptions.Add(new SortDescription(nameof(ArchiveRecordRow.LastSavedValue), ListSortDirection.Ascending));
                break;
            case 2:
                _archiveView.SortDescriptions.Add(new SortDescription(nameof(ArchiveRecordRow.ApprovalSortKey), ListSortDirection.Descending));
                _archiveView.SortDescriptions.Add(new SortDescription(nameof(ArchiveRecordRow.LastSavedValue), ListSortDirection.Descending));
                break;
            case 3:
                _archiveView.SortDescriptions.Add(new SortDescription(nameof(ArchiveRecordRow.JobTitle), ListSortDirection.Ascending));
                break;
            case 4:
                _archiveView.SortDescriptions.Add(new SortDescription(nameof(ArchiveRecordRow.OperationNumber), ListSortDirection.Ascending));
                break;
            default:
                _archiveView.SortDescriptions.Add(new SortDescription(nameof(ArchiveRecordRow.LastSavedValue), ListSortDirection.Descending));
                break;
        }

        var visible = _archiveView.Cast<object>().Count();
        View<System.Windows.Controls.TextBlock>("ArchiveCountText").Text = visible == Context.State.ArchiveRecords.Count ? $"{Context.State.ArchiveRecords.Count} kayıt" : $"{visible} / {Context.State.ArchiveRecords.Count} kayıt";
    }

    internal void ArchiveSearch_TextChanged(object sender, TextChangedEventArgs e) => ApplyArchiveView();
    internal void ArchiveFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyArchiveView();
    internal void ArchiveSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyArchiveView();
    internal void ArchiveGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (View<System.Windows.Controls.DataGrid>("ArchiveGrid").SelectedItem is ArchiveRecordRow)
            OpenArchive_Click(sender, new RoutedEventArgs());
    }

    internal void ArchiveRowEdit_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        if (sender is not FrameworkElement { DataContext: ArchiveRecordRow row })
            return;
        View<System.Windows.Controls.DataGrid>("ArchiveGrid").SelectedItem = row;
        OpenArchive_Click(sender, e);
    }

    internal void ArchiveRowFolder_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        if (sender is not FrameworkElement { DataContext: ArchiveRecordRow row })
            return;
        try
        {
            var folder = Context.Storage.OutputFolderFor(row.RecordId);
            System.IO.Directory.CreateDirectory(folder);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(folder) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show("Belge klasörü açılamadı: " + ex.Message, "DoğruTemin", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal void ArchiveRowDelete_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        if (sender is not FrameworkElement { DataContext: ArchiveRecordRow row })
            return;
        View<System.Windows.Controls.DataGrid>("ArchiveGrid").SelectedItem = row;
        DeleteArchive_Click(sender, e);
    }

    internal void OpenArchive_Click(object sender, RoutedEventArgs e)
    {
        if (View<System.Windows.Controls.DataGrid>("ArchiveGrid").SelectedItem is not ArchiveRecordRow row)
        {
            MessageBox.Show("Açmak için arşivden bir işlem seçiniz.", "DoğruTemin", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (!Context.Workspace.ConfirmRecordSwitch())
            return;
        var data = Context.Storage.LoadRecord(row.RecordId, out var warning);
        if (data is null)
        {
            MessageBox.Show(warning ?? "İşlem açılamadı.", "DoğruTemin", MessageBoxButton.OK, MessageBoxImage.Warning);
            RefreshArchiveRecords();
            return;
        }

        Context.State.SuppressDirtyTracking = true;
        Context.Workspace.ApplyDataToUi(data);
        Context.Catalog.ClearFirmForm();
        Context.Catalog.ClearProductForm();
        Context.State.SuppressDirtyTracking = false;
        Context.Storage.ActivateRecord(data);
        Context.Workspace.UpdateDerivedState(data);
        Context.Session.BeginArchiveEdit(data.RecordId);
        Context.Workspace.SetSavedStatus(data.LastSavedAt);
        Context.Shell.ShowView("General");
    }

    internal void DeleteArchive_Click(object sender, RoutedEventArgs e)
    {
        if (View<System.Windows.Controls.DataGrid>("ArchiveGrid").SelectedItem is not ArchiveRecordRow row)
        {
            MessageBox.Show("Silmek için arşivden bir işlem seçiniz.", "DoğruTemin", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (ArchiveWorkflow.CanDelete(row.RecordId, Context.Session.RecordId).Status == ArchiveWorkflowStatus.Blocked)
        {
            MessageBox.Show("Aktif işlem arşivden silinemez. Önce YENİ İŞLEM başlatın veya başka bir işlemi açın.", "DoğruTemin", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var answer = MessageBox.Show($"'{row.JobTitle}' kaydı arşivden silinsin mi?\n\nKayıt güvenli 'Silinenler' yedeğine taşınacaktır.", "DoğruTemin - Arşiv", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (answer != MessageBoxResult.Yes)
            return;
        var result = Context.ArchiveWorkflow.Delete(row.RecordId, Context.Session.RecordId);
        if (result.Status == ArchiveWorkflowStatus.Blocked)
        {
            MessageBox.Show("Aktif işlem arşivden silinemez. Önce YENİ İŞLEM başlatın veya başka bir işlemi açın.", "DoğruTemin", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (result.Status == ArchiveWorkflowStatus.Failed)
        {
            MessageBox.Show("Arşiv kaydı silinemedi.", "DoğruTemin", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        RefreshArchiveRecords();
    }
}
