using DogruTemin.Wpf.Features;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DogruTemin.Wpf;
/// <summary>Ek Ders Hakediş'in hücre odaklı, ortalanmış hibrit tablo düzeni.</summary>
internal static class HybridTableService
{
    internal static void Apply(Window window)
    {
        foreach (var grid in ResponsiveLayoutService.LogicalNodes(window).OfType<DataGrid>())
        {
            grid.RowHeight = (double)window.FindResource("AtlasRowHeight");
            grid.GridLinesVisibility = DataGridGridLinesVisibility.All;
            grid.HorizontalGridLinesBrush = (Brush)window.FindResource("AtlasLineBrush");
            grid.VerticalGridLinesBrush = grid.HorizontalGridLinesBrush;
            grid.AlternationCount = 2;
            grid.ColumnHeaderStyle = (Style)window.FindResource("HybridGridHeader");
            grid.CanUserReorderColumns = false;
            grid.CanUserResizeRows = false;
            grid.HeadersVisibility = DataGridHeadersVisibility.Column;
            grid.FrozenColumnCount = 1;
            var cells = new Style(typeof(DataGridCell), grid.CellStyle);
            cells.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
            cells.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center));
            cells.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(3, 0, 3, 0)));
            grid.CellStyle = cells;
            foreach (var column in grid.Columns.OfType<DataGridTextColumn>())
            {
                var text = new Style(typeof(TextBlock), column.ElementStyle);
                text.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));
                text.Setters.Add(new Setter(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Stretch));
                text.Setters.Add(new Setter(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center));
                column.ElementStyle = text;
                var editor = new Style(typeof(TextBox), column.EditingElementStyle);
                editor.Setters.Add(new Setter(TextBox.TextAlignmentProperty, TextAlignment.Center));
                editor.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center));
                editor.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(3, 0, 3, 0)));
                column.EditingElementStyle = editor;
            }

            grid.LoadingRow += (_, args) =>
            {
                args.Row.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var text in UiTree.Descendants<TextBlock>(args.Row))
                    {
                        text.TextAlignment = TextAlignment.Center;
                        text.VerticalAlignment = VerticalAlignment.Center;
                        text.HorizontalAlignment = HorizontalAlignment.Stretch;
                    }

                    foreach (var input in UiTree.Descendants<TextBox>(args.Row))
                    {
                        input.TextAlignment = TextAlignment.Center;
                        input.VerticalContentAlignment = VerticalAlignment.Center;
                    }
                });
            };
        }
    }
}
