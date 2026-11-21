using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DogruTemin.Wpf;
namespace DogruTemin.Tests;
internal static class AtlasThemeAcceptance
{
 internal static void Review(MainWindow window)
 {
  Assert.Equal(new CornerRadius(15), Assert.IsType<CornerRadius>(window.TryFindResource("AtlasCardRadius")));
  Assert.Equal(Color.FromRgb(143,185,194), Assert.IsType<SolidColorBrush>(window.TryFindResource("AtlasAccentBrush")).Color);
  Assert.Equal(36, ((TextBox)window.FindName("ProductNameBox")).Height);
  Assert.Equal(14, ((TextBox)window.FindName("ProductNameBox")).FontSize);
  Assert.True(((Control)window.FindName("ProductQuantityBox")).TabIndex < ((Control)window.FindName("ProductUnitBox")).TabIndex);
  Assert.Equal(HorizontalAlignment.Left, UiTree.FindAncestor<Viewbox>((TextBlock)window.FindName("HomeApproximateTotalText"))!.HorizontalAlignment);
  var products = (DataGrid)window.FindName("ProductGrid");
  Assert.Equal(36, products.RowHeight);
  Assert.Equal(DataGridGridLinesVisibility.All, products.GridLinesVisibility);
  Assert.Equal(2, products.AlternationCount);
  WorkspaceTestDriver.Invoke(window,"ShowView",new object[]{"General"});
  window.Width=1920; window.Height=1080; window.UpdateLayout();
  var editor=(FrameworkElement)window.FindName("AtlasProductEditor");
  var list=(FrameworkElement)window.FindName("AtlasProductList");
  Assert.True(list.TranslatePoint(new Point(),window).X > editor.TranslatePoint(new Point(),window).X + editor.ActualWidth,
    "Atlas geniÅŸ ekranda giriÅŸ kartÄ± ve tabloyu yan yana gÃ¶stermeli.");
  Assert.False(window.Session.HasUnsavedWork);
 }
}
