using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;

namespace DogruTemin.Wpf.Features.Catalog;
public partial class ProductsPage : UserControl
{
    public ProductsPage() => InitializeComponent();
    private void AddOrUpdateProduct_Click(object sender, RoutedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Catalog.CatalogModule.AddOrUpdateProduct_Click), sender, e);
    private void AnyGlobalField_TextChanged(object sender, TextChangedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Documents.DocumentsModule.AnyGlobalField_TextChanged), sender, e);
    private void ApprovalNoBox_LostFocus(object sender, RoutedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Documents.DocumentsModule.ApprovalNoBox_LostFocus), sender, e);
    private void ProductDelete_Click(object sender, RoutedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Catalog.CatalogModule.ProductDelete_Click), sender, e);
    private void ProductEdit_Click(object sender, RoutedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Catalog.CatalogModule.ProductEdit_Click), sender, e);
    private void TransientProductEditor_SelectionChanged(object sender, SelectionChangedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Catalog.CatalogModule.TransientProductEditor_SelectionChanged), sender, e);
    private void TransientProductEditor_TextChanged(object sender, TextChangedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Catalog.CatalogModule.TransientProductEditor_TextChanged), sender, e);
}
