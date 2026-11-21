using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;

namespace DogruTemin.Wpf.Features.Archive;
public partial class ArchivePage : UserControl
{
    public ArchivePage() => InitializeComponent();
    private void ArchiveFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Archive.ArchiveModule.ArchiveFilter_SelectionChanged), sender, e);
    private void ArchiveGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Archive.ArchiveModule.ArchiveGrid_MouseDoubleClick), sender, e);
    private void ArchiveRowDelete_Click(object sender, RoutedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Archive.ArchiveModule.ArchiveRowDelete_Click), sender, e);
    private void ArchiveRowEdit_Click(object sender, RoutedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Archive.ArchiveModule.ArchiveRowEdit_Click), sender, e);
    private void ArchiveRowFolder_Click(object sender, RoutedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Archive.ArchiveModule.ArchiveRowFolder_Click), sender, e);
    private void ArchiveSearch_TextChanged(object sender, TextChangedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Archive.ArchiveModule.ArchiveSearch_TextChanged), sender, e);
    private void ArchiveSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Archive.ArchiveModule.ArchiveSort_SelectionChanged), sender, e);
}
