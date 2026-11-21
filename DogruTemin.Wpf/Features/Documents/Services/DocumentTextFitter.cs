using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace DogruTemin.Wpf.Services;
/// <summary>Fits complete text into its actual printable cell before preview, PDF, or printing.</summary>
internal static class DocumentTextFitter
{
    internal static void Fit(FixedPage page)
    {
        for (var pass = 0; pass < 4; pass++)
        {
            Layout(page);
            // Structured forms must fit the page before individual cells are measured.
            if (page.Children.OfType<System.Windows.Controls.Grid>().Any())
                FitOverflowingPage(page);
            Layout(page);
            var changed = false;
            foreach (var block in TextBlocks(page).ToArray())
            {
                if (string.IsNullOrEmpty(block.Text) || block.ActualWidth <= 0 || block.ActualHeight <= 0)
                    continue;
                var area = PrintableBounds(page, block);
                if (area.Width <= 0 || area.Height <= 0)
                    continue;
                var unit = block.TransformToAncestor(page).TransformBounds(new Rect(0, 0, 1, 1));
                area = new Rect(0, 0, area.Width / Math.Max(.00001, unit.Width), area.Height / Math.Max(.00001, unit.Height));
                var original = block.FontSize;
                if (Fits(block, original, area))
                    continue;
                double low = .05, high = original;
                for (var i = 0; i < 18; i++)
                {
                    var candidate = (low + high) / 2;
                    if (Fits(block, candidate, area))
                        low = candidate;
                    else
                        high = candidate;
                }

                block.FontSize = low;
                if (double.IsFinite(block.LineHeight))
                    block.LineHeight = Math.Max(.05, block.LineHeight * low / original);
                changed = true;
            }

            if (!changed)
                break;
        }

        Layout(page);
        FitOverflowingPage(page);
        Layout(page);
    }

    private static void FitOverflowingPage(FixedPage page)
    {
        var allText = Rect.Empty;
        foreach (var block in TextBlocks(page))
        {
            if (string.IsNullOrWhiteSpace(block.Text))
                continue;
            allText.Union(block.TransformToAncestor(page).TransformBounds(new Rect(block.RenderSize)));
            for (var parent = VisualTreeHelper.GetParent(block); parent is not null && parent != page; parent = VisualTreeHelper.GetParent(parent))
                if (parent is Border border)
                    allText.Union(border.TransformToAncestor(page).TransformBounds(new Rect(border.RenderSize)));
        }

        if (allText.IsEmpty || allText.Left >= 0 && allText.Top >= 0 && allText.Right <= page.Width - 12 && allText.Bottom <= page.Height - 12)
            return;
        var offsetX = Math.Max(0, -allText.Left);
        var offsetY = Math.Max(0, -allText.Top);
        var scale = Math.Min(1, Math.Min(page.Width / Math.Max(page.Width, allText.Right + offsetX + 12), page.Height / Math.Max(page.Height, allText.Bottom + offsetY + 12)));
        foreach (UIElement child in page.Children)
        {
            var transform = new TransformGroup();
            transform.Children.Add(child.RenderTransform.Clone());
            transform.Children.Add(new TranslateTransform(offsetX, offsetY));
            transform.Children.Add(new ScaleTransform(scale, scale));
            var left = FixedPage.GetLeft(child);
            if (!double.IsFinite(left))
                left = 0;
            transform.Children.Add(new TranslateTransform((1 - scale) * (page.Width / 2 - left), 0));
            child.RenderTransform = transform;
        }
    }

    internal static Rect PrintableBounds(FixedPage page, TextBlock block)
    {
        var bounds = block.TransformToAncestor(page).TransformBounds(new Rect(block.RenderSize));
        bounds.Intersect(new Rect(0, 0, page.Width, page.Height));
        for (var parent = VisualTreeHelper.GetParent(block); parent is not null && parent != page; parent = VisualTreeHelper.GetParent(parent))
        {
            if (parent is not Border cell || cell.ActualWidth <= 0 || cell.ActualHeight <= 0)
                continue;
            var left = cell.Padding.Left + cell.BorderThickness.Left;
            var top = cell.Padding.Top + cell.BorderThickness.Top;
            var available = new Rect(left, top, Math.Max(0, cell.ActualWidth - left - cell.Padding.Right - cell.BorderThickness.Right), Math.Max(0, cell.ActualHeight - top - cell.Padding.Bottom - cell.BorderThickness.Bottom));
            bounds.Intersect(cell.TransformToAncestor(page).TransformBounds(available));
        }

        return bounds.IsEmpty ? new Rect() : bounds;
    }

    private static bool Fits(TextBlock block, double size, Rect area)
    {
        var probe = new TextBlock
        {
            Text = block.Text,
            FontFamily = block.FontFamily,
            FontWeight = block.FontWeight,
            FontStyle = block.FontStyle,
            FontStretch = block.FontStretch,
            FontSize = size,
            TextWrapping = block.TextWrapping,
            FlowDirection = block.FlowDirection
        };
        if (double.IsFinite(block.LineHeight))
            probe.LineHeight = Math.Max(.05, block.LineHeight * size / block.FontSize);
        probe.Measure(new Size(block.TextWrapping == TextWrapping.NoWrap ? double.PositiveInfinity : Math.Max(.05, area.Width), double.PositiveInfinity));
        return probe.DesiredSize.Width <= area.Width + .1 && probe.DesiredSize.Height <= area.Height + .1;
    }

    private static void Layout(FixedPage page)
    {
        page.Measure(new Size(page.Width, page.Height));
        page.Arrange(new Rect(0, 0, page.Width, page.Height));
        page.UpdateLayout();
    }

    private static IEnumerable<TextBlock> TextBlocks(DependencyObject parent)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is TextBlock text)
                yield return text;
            else
                foreach (var descendant in TextBlocks(child))
                    yield return descendant;
        }
    }
}
