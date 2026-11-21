using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;

namespace DogruTemin.Wpf.Features.Safety;
public partial class SafetyPage : UserControl
{
    public SafetyPage() => InitializeComponent();
    private void CreateDiagnosticPackage_Click(object sender, RoutedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Safety.SafetyModule.CreateDiagnosticPackage_Click), sender, e);
    private void CreateSafetyBackup_Click(object sender, RoutedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Safety.SafetyModule.CreateSafetyBackup_Click), sender, e);
    private void OpenBackupFolder_Click(object sender, RoutedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Safety.SafetyModule.OpenBackupFolder_Click), sender, e);
    private void OpenDataFolder_Click(object sender, RoutedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Safety.SafetyModule.OpenDataFolder_Click), sender, e);
    private void OpenLogFolder_Click(object sender, RoutedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Safety.SafetyModule.OpenLogFolder_Click), sender, e);
    private void RestoreSafetyBackup_Click(object sender, RoutedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Safety.SafetyModule.RestoreSafetyBackup_Click), sender, e);
    private void RunIntegrityCheck_Click(object sender, RoutedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Safety.SafetyModule.RunIntegrityCheck_Click), sender, e);
}
