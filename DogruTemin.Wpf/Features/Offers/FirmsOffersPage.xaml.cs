using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;

namespace DogruTemin.Wpf.Features.Offers;
public partial class FirmsOffersPage : UserControl
{
    public FirmsOffersPage() => InitializeComponent();
    private void AddOrUpdateFirm_Click(object sender, RoutedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Catalog.CatalogModule.AddOrUpdateFirm_Click), sender, e);
    private void OfferEditorDate_Changed(object sender, RoutedPropertyChangedEventArgs<DateTime?> e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Offers.OffersModule.OfferEditorDate_Changed), sender, e);
    private void OfferEditorText_Changed(object sender, TextChangedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Offers.OffersModule.OfferEditorText_Changed), sender, e);
    private void OfferFirmSelector_SelectionChanged(object sender, SelectionChangedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Offers.OffersModule.OfferFirmSelector_SelectionChanged), sender, e);
    private void OfferGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Offers.OffersModule.OfferGrid_CellEditEnding), sender, e);
    private void RowDelete_Click(object sender, RoutedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Catalog.CatalogModule.RowDelete_Click), sender, e);
    private void RowEdit_Click(object sender, RoutedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Catalog.CatalogModule.RowEdit_Click), sender, e);
    private void SaveOffer_Click(object sender, RoutedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Offers.OffersModule.SaveOffer_Click), sender, e);
    private void TransientFirmEditor_TextChanged(object sender, TextChangedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Catalog.CatalogModule.TransientFirmEditor_TextChanged), sender, e);
}
