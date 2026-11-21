using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;

namespace DogruTemin.Wpf.Features.Documents;
public partial class ApprovalPage : UserControl
{
    public ApprovalPage() => InitializeComponent();
    private void AnyDate_Changed(object? sender, RoutedPropertyChangedEventArgs<DateTime?> e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Documents.DocumentsModule.AnyDate_Changed), sender, e);
    private void AnyGlobalField_TextChanged(object sender, TextChangedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Documents.DocumentsModule.AnyGlobalField_TextChanged), sender, e);
}
