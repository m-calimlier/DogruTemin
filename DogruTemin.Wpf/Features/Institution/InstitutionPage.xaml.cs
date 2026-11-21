using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;

namespace DogruTemin.Wpf.Features.Institution;
public partial class InstitutionPage : UserControl
{
    public InstitutionPage() => InitializeComponent();
    private void AnyGlobalField_TextChanged(object sender, TextChangedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Documents.DocumentsModule.AnyGlobalField_TextChanged), sender, e);
    private void CommissionGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Documents.DocumentsModule.CommissionGrid_CellEditEnding), sender, e);
    private void CommissionTitle_SelectionChanged(object sender, SelectionChangedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Documents.DocumentsModule.CommissionTitle_SelectionChanged), sender, e);
    private void SaveApplicationSettings_Click(object sender, RoutedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Institution.InstitutionModule.SaveApplicationSettings_Click), sender, e);
}
