using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DogruTemin.Wpf;
public sealed class NavigationIcon : Viewbox
{
    public static readonly DependencyProperty KindProperty = DependencyProperty.Register(nameof(Kind), typeof(string), typeof(NavigationIcon), new PropertyMetadata("Home", (sender, _) => ((NavigationIcon)sender).Draw()));
    public string Kind { get => (string)GetValue(KindProperty); set => SetValue(KindProperty, value); }

    public NavigationIcon()
    {
        Stretch = Stretch.Uniform;
        IsHitTestVisible = false;
        Draw();
    }

    private void Draw()
    {
        var geometry = Kind switch
        {
            "Institution" => "M3,9 L12,3 21,9 Z M5,11 L5,19 M10,11 L10,19 M14,11 L14,19 M19,11 L19,19 M3,21 L21,21",
            "Products" => "M3,7 L12,3 21,7 21,18 12,22 3,18 Z M3,7 L12,11 21,7 M12,11 L12,22 M8,5 L17,9",
            "Firms" => "M3,21 L3,7 10,7 10,21 M10,21 L10,3 21,3 21,21 Z M6,11 L7,11 M6,15 L7,15 M14,7 L17,7 M14,11 L17,11 M14,15 L17,15 M14,21 L14,18 17,18 17,21",
            "Approval" => "M5,3 L15,3 20,8 20,21 5,21 Z M15,3 L15,8 20,8 M8,12 L16,12 M8,16 L11,19 17,13",
            "Calendar" => "M3,6 L21,6 21,21 3,21 Z M3,10 L21,10 M7,3 L7,8 M17,3 L17,8 M7,14 L9,14 M13,14 L15,14 M7,18 L9,18 M13,18 L15,18",
            "Output" => "M6,8 L6,3 18,3 18,8 M6,17 L3,17 3,9 21,9 21,17 18,17 M6,14 L18,14 18,22 6,22 Z M17,11 L18,11",
            "Archive" => "M3,4 L21,4 21,9 3,9 Z M5,9 L5,21 19,21 19,9 M9,13 L15,13",
            "Chart" => "M3,3 L3,21 22,21 M7,17 L7,12 M12,17 L12,8 M17,17 L17,4",
            "Settings" => "M9,3 L15,3 16,6 19,7 22,11 20,15 17,16 16,20 11,22 7,20 6,17 3,15 2,10 5,7 8,6 Z M16,12 A4,4 0 1 1 8,12 A4,4 0 1 1 16,12",
            "Shield" => "M12,2 L21,6 20,14 Q18,20 12,23 Q6,20 4,14 L3,6 Z M8,12 L11,15 17,9",
            _ => "M2,11 L12,3 22,11 M5,10 L5,21 10,21 10,15 14,15 14,21 19,21 19,10"
        };
        var canvas = new Canvas
        {
            Width = 24,
            Height = 24
        };
        var path = new System.Windows.Shapes.Path
        {
            Data = Geometry.Parse(geometry),
            StrokeThickness = 1.65,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeLineJoin = PenLineJoin.Round
        };
        path.SetResourceReference(Shape.StrokeProperty, "MetricGold");
        canvas.Children.Add(path);
        Child = canvas;
    }
}
