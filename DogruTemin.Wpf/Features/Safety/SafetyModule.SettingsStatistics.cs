using AppSettings = DogruTemin.Core.Models.ApplicationSettings;
using DogruTemin.Core.Services;
using DogruTemin.Wpf.Models;
using DogruTemin.Wpf.Services;
using Microsoft.Win32;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DogruTemin.Wpf.Features.Safety;
internal sealed partial class SafetyModule
{
    internal void OpenDataFolder_Click(object sender, RoutedEventArgs e) => OpenFolder(Context.Storage.DataFolder);
    internal void OpenBackupFolder_Click(object sender, RoutedEventArgs e) => OpenFolder(Context.Storage.BackupFolder);
    internal void OpenLogFolder_Click(object sender, RoutedEventArgs e) => OpenFolder(Context.Storage.LogFolder);
    internal void CreateDiagnosticPackage_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var path = DiagnosticPackageService.Create(Context.SafetyService);
            UiNotificationService.Show(Context.Window, "Tanılama paketi oluşturuldu:\n" + path, TimeSpan.FromSeconds(6));
        }
        catch (Exception ex)
        {
            MessageBox.Show("Tanılama paketi oluşturulamadı:\n\n" + ex.Message, "DoğruTemin - Tanılama", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal void UpdateSafetyPanel(SafetyCheckResult result)
    {
        if (View<System.Windows.Controls.TextBlock>("AboutVersionText")is null || View<System.Windows.Controls.TextBlock>("DataIntegrityText")is null)
            return;
        View<System.Windows.Controls.TextBlock>("AboutVersionText").Text = $"DoğruTemin {AppVersionInfo.DisplayVersion}";
        View<System.Windows.Controls.TextBlock>("AboutRuntimeText").Text = $"Windows {(Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit")}  •  .NET {ApplicationSafetyService.RuntimeDescription}";
        View<System.Windows.Controls.TextBlock>("AboutDataFolderText").Text = Context.Storage.RootFolder;
        View<System.Windows.Controls.TextBlock>("DataIntegrityText").Text = result.Message;
        View<System.Windows.Controls.TextBlock>("LastIntegrityCheckText").Text = $"Son kontrol: {DateTime.Now:dd.MM.yyyy HH:mm}";
        View<System.Windows.Controls.TextBlock>("DataIntegrityText").Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(result.IsHealthy ? "#157347" : "#A72B37"));
    }

    internal void RunIntegrityCheck_Click(object sender, RoutedEventArgs e)
    {
        var result = Context.SafetyService.CheckIntegrity();
        UpdateSafetyPanel(result);
        if (result.IsHealthy)
            UiNotificationService.Show(Context.Window, result.Message, TimeSpan.FromSeconds(4));
        else
            MessageBox.Show(result.Message, "DoğruTemin - Veri Bütünlüğü", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    internal void CreateSafetyBackup_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (Context.Workspace.HasUnsavedWork())
            {
                var answer = MessageBox.Show("Aktif işlemde kaydedilmemiş değişiklikler var. Güvenlik yedeğine son değişikliklerin de girmesi için önce kayıt yapılsın mı?", "DoğruTemin - Güvenlik Yedeği", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (answer == MessageBoxResult.Cancel)
                    return;
                if (answer == MessageBoxResult.Yes)
                {
                    Context.Workspace.Save_Click(Context.Window, new RoutedEventArgs());
                    if (Context.Workspace.HasUnsavedWork())
                        return;
                }
            }

            var path = Context.SafetyService.CreateManualSafetyBackup();
            UiNotificationService.Show(Context.Window, $"Güvenlik yedeği oluşturuldu:\n{path}", TimeSpan.FromSeconds(5));
            UpdateSafetyPanel(Context.SafetyService.CheckIntegrity());
        }
        catch (Exception ex)
        {
            MessageBox.Show("Güvenlik yedeği oluşturulamadı: " + ex.Message, "DoğruTemin", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal void RestoreSafetyBackup_Click(object sender, RoutedEventArgs e)
    {
        if (!Context.Workspace.ConfirmUnsavedWork("yedekten geri yüklemeye devam et", "DoğruTemin - Yedekten Geri Yükle"))
            return;
        var dialog = new WorkspaceOpenFileDialog
        {
            Title = "DoğruTemin Yedeğini Seç",
            Filter = "DoğruTemin ZIP yedeği (*.zip)|*.zip|Tüm dosyalar (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false
        };
        if (dialog.ShowDialog(Context.Window) != true)
            return;
        var confirm = MessageBox.Show("Seçilen yedek geri yüklenecek. Mevcut veriler değiştirilmeden önce otomatik güvenlik yedeği oluşturulacaktır.\n\nDevam edilsin mi?", "DoğruTemin - Yedekten Geri Yükle", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes)
            return;
        var result = Context.SafetyService.RestoreFromBackup(dialog.FileName);
        if (!result.Success)
        {
            MessageBox.Show(result.Message, "DoğruTemin - Yedekten Geri Yükle", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var message = result.Message;
        if (!string.IsNullOrWhiteSpace(result.SafetyBackupPath))
            message += "\n\nGeri yükleme öncesi mevcut verinin güvenlik kopyası:\n" + result.SafetyBackupPath!;
        message += "\n\nYeni verilerin güvenli biçimde yüklenmesi için DoğruTemin yeniden başlatılacak.";
        MessageBox.Show(message, "DoğruTemin - Yedekten Geri Yükle", MessageBoxButton.OK, MessageBoxImage.Information);
        Context.State.CloseApproved = true;
        RecoverySessionService.EndCleanly();
        RestartApplication();
    }

    internal static void RestartApplication()
    {
        try
        {
            var exe = Environment.ProcessPath;
            if (!string.IsNullOrWhiteSpace(exe) && File.Exists(exe))
            {
                App.ReleaseSingleInstanceForRestart();
                Process.Start(new ProcessStartInfo { FileName = exe, UseShellExecute = true });
            }
        }
        catch
        {
        }

        Application.Current.Shutdown();
    }

    internal static void OpenFolder(string folder)
    {
        try
        {
            Directory.CreateDirectory(folder);
            Process.Start(new ProcessStartInfo { FileName = folder, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show("Klasör açılamadı: " + ex.Message, "DoğruTemin", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
