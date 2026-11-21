using System.Windows;
using DogruTemin.Wpf.Models;
namespace DogruTemin.Wpf;
public partial class DocumentPreviewWindow : Window
{
    public DocumentPreviewWindow(DocumentPreviewItem item)
    {
        InitializeComponent();
        DataContext = item;
        Width = System.Math.Min(1320, SystemParameters.WorkArea.Width - 60);
        Height = System.Math.Min(940, SystemParameters.WorkArea.Height - 60);
        TextSharpnessService.Apply(this);
    }
    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
