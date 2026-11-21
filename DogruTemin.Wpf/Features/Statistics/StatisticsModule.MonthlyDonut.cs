using DogruTemin.Core.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DogruTemin.Wpf.Features.Statistics;
internal sealed partial class StatisticsModule
{
    private sealed record StatisticsDonutItem(string Month, int Count, decimal Total);
    internal void RefreshMonthlyDonut()
    {
        View<System.Windows.Controls.WrapPanel>("StatisticsDonutLegendPanel").Children.Clear();
        View<System.Windows.Controls.Canvas>("StatisticsDonutLabelsCanvas").Children.Clear();
        var distribution = Context.State.MonthlyStatistics.Where(row => row.Count > 0).Select(row => new StatisticsDonutItem(row.Month, row.Count, row.FinalTotal)).ToArray();
        var total = distribution.Sum(item => item.Count);
        View<System.Windows.Controls.TextBlock>("StatisticsDonutTotalText").Text = total.ToString(System.Globalization.CultureInfo.GetCultureInfo("tr-TR"));
        View<System.Windows.Controls.TextBlock>("StatisticsDonutEmptyState").Visibility = total > 0 ? Visibility.Collapsed : Visibility.Visible;
        View<System.Windows.Controls.Canvas>("StatisticsDonutChart").Children.Clear();
        var angle = -90d;
        foreach (var item in distribution)
        {
            var percent = item.Count * 100m / total;
            var sweep = item.Count * 360d / total;
            var color = StatisticsBrush(item.Month);
            var shape = new System.Windows.Shapes.Path
            {
                Data = MonthlyRingSlice(angle, sweep),
                Fill = color,
                Stroke = Brushes.White,
                StrokeThickness = 2.4,
                ToolTip = $"{item.Month}: {item.Count} işlem ({DonutPercentageLayout.FormatPercent(percent)})"};
            View<System.Windows.Controls.Canvas>("StatisticsDonutChart").Children.Add(shape);
            angle += sweep;
            AddStatisticsLegendItem(item, percent, StatisticsBrush(item.Month));
        }

        RefreshStatisticsDonutLabels(distribution, total);
    }

    internal static StreamGeometry MonthlyRingSlice(double start, double sweep)
    {
        Point At(double radius, double angle) => new(164 + radius * Math.Cos(angle * Math.PI / 180), 164 + radius * Math.Sin(angle * Math.PI / 180));
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            context.BeginFigure(At(157, start), true, true);
            if (sweep >= 359.999)
            {
                context.ArcTo(At(157, start + 180), new Size(157, 157), 0, false, SweepDirection.Clockwise, true, false);
                context.ArcTo(At(157, start + 360), new Size(157, 157), 0, false, SweepDirection.Clockwise, true, false);
            }
            else
                context.ArcTo(At(157, start + sweep), new Size(157, 157), 0, sweep > 180, SweepDirection.Clockwise, true, false);
            context.LineTo(At(82, start + sweep), true, false);
            if (sweep >= 359.999)
            {
                context.ArcTo(At(82, start + 180), new Size(82, 82), 0, false, SweepDirection.Counterclockwise, true, false);
                context.ArcTo(At(82, start), new Size(82, 82), 0, false, SweepDirection.Counterclockwise, true, false);
            }
            else
                context.ArcTo(At(82, start), new Size(82, 82), 0, sweep > 180, SweepDirection.Counterclockwise, true, false);
        }

        geometry.Freeze();
        return geometry;
    }

    internal Color StatisticsSkColor(string month)
    {
        Color[] palette =
        {
            Color.FromRgb(218, 165, 32),
            Color.FromRgb(70, 130, 180),
            Color.FromRgb(147, 112, 219),
            Color.FromRgb(60, 179, 113),
            Color.FromRgb(112, 128, 144),
            Color.FromRgb(230, 126, 34),
            Color.FromRgb(30, 170, 170),
            Color.FromRgb(218, 112, 155),
            Color.FromRgb(90, 150, 230),
            Color.FromRgb(160, 170, 65),
            Color.FromRgb(184, 125, 220),
            Color.FromRgb(60, 195, 210)
        };
        var index = Context.State.MonthlyStatistics.First(row => row.Month == month).MonthNumber - 1;
        return palette[index];
    }

    internal SolidColorBrush StatisticsBrush(string month)
    {
        var color = StatisticsSkColor(month);
        return new SolidColorBrush(Color.FromRgb(color.R, color.G, color.B));
    }

    internal static Color StatisticsDataLabelColor(Color background) => .299 * background.R + .587 * background.G + .114 * background.B >= 150 ? Color.FromRgb(3, 28, 56) : Colors.White;
    private void RefreshStatisticsDonutLabels(IReadOnlyList<StatisticsDonutItem> distribution, decimal total)
    {
        View<System.Windows.Controls.Canvas>("StatisticsDonutLabelsCanvas").Children.Clear();
        if (total <= 0)
            return;
        foreach (var label in DonutPercentageLayout.Plan(distribution.Select(item => (decimal)item.Count).ToArray()))
        {
            var sliceColor = StatisticsSkColor(distribution[label.Index].Month);
            var sliceBrush = StatisticsBrush(distribution[label.Index].Month);
            if (label.IsCallout)
            {
                var edgeX = label.Left < DonutPercentageLayout.CanvasWidth / 2 ? label.Left + DonutPercentageLayout.LabelWidth : label.Left;
                var middleX = edgeX + (label.Left < DonutPercentageLayout.CanvasWidth / 2 ? 2 : -2);
                var radialX = DonutPercentageLayout.CanvasWidth / 2 + (label.AnchorX - DonutPercentageLayout.CanvasWidth / 2) * 172 / 157;
                var radialY = DonutPercentageLayout.CanvasHeight / 2 + (label.AnchorY - DonutPercentageLayout.CanvasHeight / 2) * 172 / 157;
                var leader = new System.Windows.Shapes.Polyline
                {
                    Stroke = sliceBrush,
                    StrokeThickness = 1.8,
                    StrokeLineJoin = PenLineJoin.Round,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round,
                    Points = new PointCollection
                    {
                        new(label.AnchorX, label.AnchorY),
                        new Point(radialX, radialY),
                        new Point(middleX, radialY),
                        new Point(middleX, label.Top + 12),
                        new Point(edgeX, label.Top + 12)
                    },
                    IsHitTestVisible = false
                };
                View<System.Windows.Controls.Canvas>("StatisticsDonutLabelsCanvas").Children.Add(leader);
                var dot = new System.Windows.Shapes.Ellipse
                {
                    Width = 6,
                    Height = 6,
                    Fill = sliceBrush,
                    Stroke = Brushes.White,
                    StrokeThickness = .7,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(dot, label.AnchorX - 3);
                Canvas.SetTop(dot, label.AnchorY - 3);
                View<System.Windows.Controls.Canvas>("StatisticsDonutLabelsCanvas").Children.Add(dot);
            }

            var text = new TextBlock
            {
                Text = label.Text,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = StatisticsDataLabelBrush(sliceColor)
            };
            var badge = new Border
            {
                Width = DonutPercentageLayout.LabelWidth,
                Height = DonutPercentageLayout.LabelHeight,
                Child = text,
                CornerRadius = new CornerRadius(5),
                Background = label.IsCallout ? sliceBrush : Brushes.Transparent,
                BorderBrush = label.IsCallout ? sliceBrush : Brushes.Transparent,
                BorderThickness = new Thickness(label.IsCallout ? .7 : 0)
            };
            Canvas.SetLeft(badge, label.Left);
            Canvas.SetTop(badge, label.Top);
            View<System.Windows.Controls.Canvas>("StatisticsDonutLabelsCanvas").Children.Add(badge);
        }
    }

    internal static SolidColorBrush StatisticsDataLabelBrush(Color background)
    {
        var color = StatisticsDataLabelColor(background);
        return new SolidColorBrush(Color.FromRgb(color.R, color.G, color.B));
    }

    private void AddStatisticsLegendItem(StatisticsDonutItem item, decimal percentage, SolidColorBrush brush)
    {
        var row = new Grid
        {
            Width = 245,
            Margin = new Thickness(0, 0, 0, 16)
        };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(22) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var marker = new Border
        {
            Width = 15,
            Height = 15,
            CornerRadius = new CornerRadius(4),
            Background = brush,
            VerticalAlignment = VerticalAlignment.Center
        };
        var label = new StackPanel
        {
            Margin = new Thickness(10, 0, 12, 0)
        };
        label.Children.Add(new TextBlock { Text = item.Month, Foreground = new SolidColorBrush(Color.FromRgb(239, 244, 250)), FontSize = 14, FontWeight = FontWeights.SemiBold });
        label.Children.Add(new TextBlock { Text = $"{item.Count} işlem", Foreground = new SolidColorBrush(Color.FromRgb(185, 203, 218)), FontSize = 13.5 });
        var value = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Right
        };
        value.Children.Add(new TextBlock { Text = NumberParser.Money(item.Total), Foreground = new SolidColorBrush(Color.FromRgb(239, 244, 250)), FontSize = 14, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Right });
        value.Children.Add(new TextBlock { Text = DonutPercentageLayout.FormatPercent(percentage), Foreground = new SolidColorBrush(Color.FromRgb(232, 199, 120)), FontSize = 12.5, FontWeight = FontWeights.SemiBold, HorizontalAlignment = HorizontalAlignment.Right });
        Grid.SetColumn(label, 1);
        Grid.SetColumn(value, 2);
        row.Children.Add(marker);
        row.Children.Add(label);
        row.Children.Add(value);
        View<System.Windows.Controls.WrapPanel>("StatisticsDonutLegendPanel").Children.Add(row);
    }
}
