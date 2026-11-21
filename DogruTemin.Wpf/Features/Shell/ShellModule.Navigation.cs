using System.Windows;
using System.Windows.Controls;

namespace DogruTemin.Wpf.Features.Shell;
internal sealed partial class ShellModule
{
    internal void SynchronizeOutputSubpageWithCurrentView()
    {
        var output = _currentView == "Output";
        View<FrameworkElement>("DatesView").Visibility = output ? Visibility.Collapsed : Visibility.Visible;
        View<FrameworkElement>("OutputView").Visibility = output ? Visibility.Visible : Visibility.Collapsed;
        View<TextBlock>("DocumentsDatesPageTitle").Text = output ? "ÇIKTILAR" : "TARİHLER";
        var description = View<TextBlock>("DocumentsDatesPageSubtitle");
        description.Text = "Diğer işlem tarihlerini yönetin. Onay Belgesi tarihi kendi panelinden düzenlenir.";
        description.Visibility = output ? Visibility.Collapsed : Visibility.Visible;
        View<Button>("NavDocumentsDates").Tag = _currentView == "Dates" ? "Active" : null;
        View<Button>("NavOutput").Tag = output ? "Active" : null;
    }
}
