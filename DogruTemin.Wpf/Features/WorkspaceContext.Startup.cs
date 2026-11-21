using DogruTemin.Core.Modules.Archive;
using DogruTemin.Core.Modules.Records;
using DogruTemin.Core.Models;
using DogruTemin.Core.Services;
using DogruTemin.Wpf.Services;
using System;
using System.Windows;
using System.Windows.Data;

namespace DogruTemin.Wpf.Features;
internal sealed partial class WorkspaceContext
{
    internal void Initialize()
    {
        Shell.ReportStartup(22, "Arayüz bileşenleri hazırlanıyor...");
        Shell.ReportStartup(40, "Görsel kalite ve erişilebilirlik katmanı yükleniyor...");
        Window.SourceInitialized += Shell.MainWindow_SourceInitialized;
        Shell.ApplyTaskbarIcon();
        Shell.ConfigureFixedWindowForCurrentScreen();
        Shell.UpdateMaximizeRestoreButton();
        Window.DataContext = State;
        Documents.InitializeDocumentPreviews();
        HybridTableService.Apply(Window);
        Archive._archiveView = CollectionViewSource.GetDefaultView(State.ArchiveRecords);
        Archive._archiveView.Filter = Archive.ArchiveRecordMatchesFilter;
        Shell.ReportStartup(52, "Veri klasörleri ve güvenlik denetleniyor...");
        var startupSafety = SafetyService.InitializeAndCheck();
        RecoverySessionService.Begin(Storage, () => SafetyService);
        Window.Loaded += (_, _) =>
        {
            if (!string.IsNullOrWhiteSpace(RecoverySessionService.PendingNotice))
            {
                MessageBox.Show(Window, RecoverySessionService.PendingNotice!, "DoğruTemin", MessageBoxButton.OK, MessageBoxImage.Warning);
                RecoverySessionService.ClearPendingNotice();
            }
        };
        Shell.ReportStartup(64, "Ayarlar yükleniyor...");
        State.Settings = Storage.LoadSettings();
        foreach (System.Windows.Controls.ComboBoxItem item in View<System.Windows.Controls.ComboBox>("WindowSizeChoice").Items)
            if ((string)item.Tag == State.Settings.WindowSizePreference)
                View<System.Windows.Controls.ComboBox>("WindowSizeChoice").SelectedItem = item;
        Window.Loaded += (_, _) => ResponsiveLayoutService.ApplyPreference(Window, State.Settings.WindowSizePreference);
        State.SuppressDirtyTracking = true;
        Shell.ReportStartup(73, "Aktif işlem ve kayıtlar yükleniyor...");
        var loaded = Storage.Load(out var loadWarning);
        if (loaded is null)
        {
            Workspace.InitializeBlankState(State.Settings);
        }
        else
        {
            Workspace.ApplyDataToUi(loaded);
            Storage.EnsureArchived(loaded);
        }

        Shell.ReportStartup(84, "Belge ve rapor bileşenleri hazırlanıyor...");
        View<System.Windows.Controls.TextBlock>("ProductCountText").Text = $"{State.Products.Count} / 10 ürün";
        State.SuppressDirtyTracking = false;
        Workspace.UpdateDerivedState();
        Workspace.SetSavedStatus(loaded?.LastSavedAt);
        Archive.RefreshArchiveRecords();
        Statistics.RefreshStatisticsYears();
        Shell.ReportStartup(92, "Arşiv ve istatistik görünümü hazırlanıyor...");
        Safety.UpdateSafetyPanel(startupSafety);
        Shell.ShowView("Home");
        Shell.ReportStartup(97, "Hesap motoru son kontrolden geçiyor...");
        var selfTest = EngineSelfTest.Run();
        if (!selfTest.Success)
            MessageBox.Show("C# hesap motoru öz testi başarısız: " + selfTest.Message, "DoğruTemin", MessageBoxButton.OK, MessageBoxImage.Error);
        if (!string.IsNullOrWhiteSpace(loadWarning))
            MessageBox.Show(loadWarning, "DoğruTemin - Veri kurtarma", MessageBoxButton.OK, MessageBoxImage.Warning);
        if (!startupSafety.IsHealthy)
            MessageBox.Show(startupSafety.Message, "DoğruTemin - Veri Güvenliği", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
