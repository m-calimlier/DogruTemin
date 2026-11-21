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
  var editor=(FrameworkElement)window.FindName("AtlasProductEditor");
  var list=(FrameworkElement)window.FindName("AtlasProductList");
  foreach (var width in new[] { 1024d, 1920d })
  {
   window.Width=width; window.Height=1080;
   window.UpdateLayout();
   window.Dispatcher.Invoke(() => {}, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
   var stacked=window.ActualWidth < 1250;
   Assert.Equal(stacked ? 2 : 0, Grid.GetRow(list));
   Assert.Equal(stacked ? 0 : 2, Grid.GetColumn(list));
   var inputOrigin=editor.TranslatePoint(new Point(),window);
   var tableOrigin=list.TranslatePoint(new Point(),window);
   Assert.True(stacked
     ? tableOrigin.Y >= inputOrigin.Y + editor.ActualHeight
     : tableOrigin.X > inputOrigin.X + editor.ActualWidth,
     $"Atlas layout must match actual viewport {window.ActualWidth} at requested width {width}.");
  }
  Assert.False(window.Session.HasUnsavedWork);
 }
}
