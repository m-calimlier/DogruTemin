using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using DogruTemin.Core.Services;
using DogruTemin.Wpf.Features;

namespace DogruTemin.Wpf;
public partial class MainWindow : Window, IWorkspaceActionSink
{
    internal WorkspaceContext Features { get; }
    internal DogruTemin.Core.Modules.Session.ProcurementSession Session => Features.Session;

    public MainWindow() : this((Action<int, string>? )null)
    {
    }

    public MainWindow(Action<int, string>? startupProgress) : this(new StorageService(), startupProgress)
    {
    }

    internal MainWindow(StorageService storage, Action<int, string>? startupProgress = null)
    {
        Features = new(this, storage, startupProgress);
        InitializeComponent();
        UiPolishService.Apply(this);
        Features.Initialize();
    }

    public new object FindName(string name) => (base.FindName(name) ?? WorkspaceElements.Find(this, name))!;
    void IWorkspaceActionSink.HandleWorkspaceAction(string action, object? sender, EventArgs args) => Features.HandleAction(action, sender, args);
    public System.Collections.ObjectModel.ObservableCollection<DogruTemin.Wpf.Models.Firm> Firms => Features.State.Firms;
    public System.Collections.ObjectModel.ObservableCollection<DogruTemin.Wpf.Models.ProductItem> Products => Features.State.Products;
    public System.Collections.ObjectModel.ObservableCollection<DogruTemin.Wpf.Models.CommissionMember> InspectionCommission => Features.State.InspectionCommission;
    public System.Collections.ObjectModel.ObservableCollection<DogruTemin.Wpf.Models.CommissionMember> MarketCommission => Features.State.MarketCommission;
    public System.Collections.ObjectModel.ObservableCollection<DogruTemin.Wpf.Models.OfferEditorRow> OfferRows => Features.State.OfferRows;
    public System.Collections.ObjectModel.ObservableCollection<DogruTemin.Wpf.Models.ArchiveRecordRow> ArchiveRecords => Features.State.ArchiveRecords;
    public System.Collections.ObjectModel.ObservableCollection<string> StatisticsYears => Features.State.StatisticsYears;
    public System.Collections.ObjectModel.ObservableCollection<DogruTemin.Wpf.Models.MonthlyStatisticRow> MonthlyStatistics => Features.State.MonthlyStatistics;
    public System.Collections.ObjectModel.ObservableCollection<DogruTemin.Wpf.Models.StatisticsOperationRow> StatisticsOperations => Features.State.StatisticsOperations;
    public System.Collections.ObjectModel.ObservableCollection<string> CommissionTitles => Features.State.CommissionTitles;
    public System.Collections.ObjectModel.ObservableCollection<DogruTemin.Wpf.Models.DocumentPreviewItem> PreparationPreviews => Features.State.PreparationPreviews;
    public System.Collections.ObjectModel.ObservableCollection<DogruTemin.Wpf.Models.DocumentPreviewItem> FinalPreviews => Features.State.FinalPreviews;

    private void Close_Click(object sender, RoutedEventArgs e) => Features.HandleAction(nameof(Features.Shell.Close_Click), sender, e);
    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Features.HandleAction(nameof(Features.Shell.Header_MouseLeftButtonDown), sender, e);
    private void MaximizeRestore_Click(object sender, RoutedEventArgs e) => Features.HandleAction(nameof(Features.Shell.MaximizeRestore_Click), sender, e);
    private void Minimize_Click(object sender, RoutedEventArgs e) => Features.HandleAction(nameof(Features.Shell.Minimize_Click), sender, e);
    private void NavAboutSafety_Click(object sender, RoutedEventArgs e) => Features.HandleAction(nameof(Features.Shell.NavAboutSafety_Click), sender, e);
    private void NavApprovalDocument_Click(object sender, RoutedEventArgs e) => Features.HandleAction(nameof(Features.Shell.NavApprovalDocument_Click), sender, e);
    private void NavArchive_Click(object sender, RoutedEventArgs e) => Features.HandleAction(nameof(Features.Shell.NavArchive_Click), sender, e);
    private void NavDocumentsDates_Click(object sender, RoutedEventArgs e) => Features.HandleAction(nameof(Features.Shell.NavDocumentsDates_Click), sender, e);
    private void NavFirmsOffers_Click(object sender, RoutedEventArgs e) => Features.HandleAction(nameof(Features.Shell.NavFirmsOffers_Click), sender, e);
    private void NavGeneral_Click(object sender, RoutedEventArgs e) => Features.HandleAction(nameof(Features.Shell.NavGeneral_Click), sender, e);
    private void NavOutput_Click(object sender, RoutedEventArgs e) => Features.HandleAction(nameof(Features.Shell.NavOutput_Click), sender, e);
    private void NavHome_Click(object sender, RoutedEventArgs e) => Features.HandleAction(nameof(Features.Shell.NavHome_Click), sender, e);
    private void NavInstitution_Click(object sender, RoutedEventArgs e) => Features.HandleAction(nameof(Features.Shell.NavInstitution_Click), sender, e);
    private void NavSettings_Click(object sender, RoutedEventArgs e) => Features.HandleAction(nameof(Features.Shell.NavSettings_Click), sender, e);
    private void NavStatistics_Click(object sender, RoutedEventArgs e) => Features.HandleAction(nameof(Features.Shell.NavStatistics_Click), sender, e);
    private void Save_Click(object sender, RoutedEventArgs e) => Features.HandleAction(nameof(Features.Workspace.Save_Click), sender, e);
    private void Window_Closing(object? sender, CancelEventArgs e) => Features.HandleAction(nameof(Features.Shell.Window_Closing), sender, e);
    private void Window_PreviewKeyDown(object sender, KeyEventArgs e) => Features.HandleAction(nameof(Features.Shell.Window_PreviewKeyDown), sender, e);
    private void Window_StateChanged(object sender, EventArgs e) => Features.HandleAction(nameof(Features.Shell.Window_StateChanged), sender, e);
}
