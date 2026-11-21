using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;

namespace DogruTemin.Wpf.Features.Institution;
public partial class SettingsPage : UserControl
{
    public SettingsPage() => InitializeComponent();
    private void ApplyWindowSettings_Click(object sender, RoutedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Institution.InstitutionModule.ApplyWindowSettings_Click), sender, e);
    private void AutoBackup_Changed(object sender, RoutedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Documents.DocumentsModule.AutoBackup_Changed), sender, e);
}
