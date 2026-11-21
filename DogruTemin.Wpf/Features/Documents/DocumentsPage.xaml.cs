using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;

namespace DogruTemin.Wpf.Features.Documents;
public partial class DocumentsPage : UserControl
{
    public DocumentsPage() => InitializeComponent();
    private void AnyDate_Changed(object? sender, RoutedPropertyChangedEventArgs<DateTime?> e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Documents.DocumentsModule.AnyDate_Changed), sender, e);
    private void DocumentThumbnail_Click(object sender, RoutedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Documents.DocumentsModule.DocumentThumbnail_Click), sender, e);
    private void OutputGroupPdf_Click(object sender, RoutedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Documents.DocumentsModule.OutputGroupPdf_Click), sender, e);
    private void SaveDates_Click(object sender, RoutedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Documents.DocumentsModule.SaveDates_Click), sender, e);
}
