using System.ComponentModel;
using System.Windows.Media;
using DogruTemin.Core.Models;

namespace DogruTemin.Wpf.Models;
public sealed class DocumentPreviewItem(OutputDocumentType type, string name) : INotifyPropertyChanged
{
    public OutputDocumentType Type { get; } = type;
    public string Name { get; } = name;
    public ImageSource? Image { get; private set; }
    public string Status { get; private set; } = "Önizleme hazırlanıyor…";

    internal void Update(ImageSource? image, string status)
    {
        Image = image;
        Status = status;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
