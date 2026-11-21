using System.Windows.Input;
using System.Windows.Controls;
using System.ComponentModel;
using DogruTemin.Core.Services;
using DogruTemin.Core.Modules.Session;
using DogruTemin.Core.Modules.Records;
using DogruTemin.Core.Modules.Archive;
using DogruTemin.Wpf.Services;
using System.Windows;

namespace DogruTemin.Wpf.Features;
internal sealed partial class WorkspaceContext
{
    internal Window Window { get; }
    internal WorkspaceState State { get; } = new();
    internal ProcurementSession Session { get; } = new();
    internal StorageService Storage { get; }
    internal ProcurementWorkflow Records { get; }
    internal ArchiveWorkflow ArchiveWorkflow { get; }
    internal ArchiveQueryService ArchiveQuery { get; }
    internal ApplicationSafetyService SafetyService { get; }
    internal Action<int, string>? StartupProgress { get; }
    internal IReadOnlyList<FeatureModule> Modules { get; }
    internal Shell.ShellModule Shell { get; }
    internal Workspace.WorkspaceModule Workspace { get; }
    internal Catalog.CatalogModule Catalog { get; }
    internal Offers.OffersModule Offers { get; }
    internal Documents.DocumentsModule Documents { get; }
    internal Archive.ArchiveModule Archive { get; }
    internal Institution.InstitutionModule Institution { get; }
    internal Statistics.StatisticsModule Statistics { get; }
    internal Safety.SafetyModule Safety { get; }

    internal WorkspaceContext(Window window, StorageService storage, Action<int, string>? startupProgress = null)
    {
        Window = window ?? throw new ArgumentNullException(nameof(window));
        Storage = storage ?? throw new ArgumentNullException(nameof(storage));
        StartupProgress = startupProgress;
        Records = new(storage);
        ArchiveWorkflow = new(storage);
        ArchiveQuery = new(storage);
        SafetyService = new(storage);
        Shell = new(this);
        Workspace = new(this);
        Catalog = new(this);
        Offers = new(this);
        Documents = new(this);
        Archive = new(this);
        Institution = new(this);
        Statistics = new(this);
        Safety = new(this);
        Modules = new FeatureModule[]
        {
            Shell,
            Workspace,
            Catalog,
            Offers,
            Documents,
            Archive,
            Institution,
            Statistics,
            Safety
        };
    }

    private readonly Dictionary<string, object> _elements = new(StringComparer.Ordinal);
    internal object? FindName(string name)
    {
        if (_elements.TryGetValue(name, out var cached))
            return cached;
        var element = WorkspaceElements.Find(Window, name);
        if (element is not null)
            _elements[name] = element;
        return element;
    }

    internal T View<T>(string name)
        where T : class => (T)FindName(name)!;
    internal void HandleAction(string action, object? sender, EventArgs args)
    {
        switch (action)
        {
            case nameof(Shell.NavOutput_Click):
                Shell.NavOutput_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Catalog.AddOrUpdateFirm_Click):
                Catalog.AddOrUpdateFirm_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Catalog.AddOrUpdateProduct_Click):
                Catalog.AddOrUpdateProduct_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Documents.AnyDate_Changed):
                Documents.AnyDate_Changed(sender!, (RoutedPropertyChangedEventArgs<DateTime?>)args);
                break;
            case nameof(Documents.AnyGlobalField_TextChanged):
                Documents.AnyGlobalField_TextChanged(sender!, (TextChangedEventArgs)args);
                break;
            case nameof(Institution.ApplyWindowSettings_Click):
                Institution.ApplyWindowSettings_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Documents.ApprovalNoBox_LostFocus):
                Documents.ApprovalNoBox_LostFocus(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Archive.ArchiveFilter_SelectionChanged):
                Archive.ArchiveFilter_SelectionChanged(sender!, (SelectionChangedEventArgs)args);
                break;
            case nameof(Archive.ArchiveGrid_MouseDoubleClick):
                Archive.ArchiveGrid_MouseDoubleClick(sender!, (MouseButtonEventArgs)args);
                break;
            case nameof(Archive.ArchiveRowDelete_Click):
                Archive.ArchiveRowDelete_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Archive.ArchiveRowEdit_Click):
                Archive.ArchiveRowEdit_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Archive.ArchiveRowFolder_Click):
                Archive.ArchiveRowFolder_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Archive.ArchiveSearch_TextChanged):
                Archive.ArchiveSearch_TextChanged(sender!, (TextChangedEventArgs)args);
                break;
            case nameof(Archive.ArchiveSort_SelectionChanged):
                Archive.ArchiveSort_SelectionChanged(sender!, (SelectionChangedEventArgs)args);
                break;
            case nameof(Documents.AutoBackup_Changed):
                Documents.AutoBackup_Changed(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Shell.Close_Click):
                Shell.Close_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Documents.CommissionGrid_CellEditEnding):
                Documents.CommissionGrid_CellEditEnding(sender!, (DataGridCellEditEndingEventArgs)args);
                break;
            case nameof(Documents.CommissionTitle_SelectionChanged):
                Documents.CommissionTitle_SelectionChanged(sender!, (SelectionChangedEventArgs)args);
                break;
            case nameof(Safety.CreateDiagnosticPackage_Click):
                Safety.CreateDiagnosticPackage_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Safety.CreateSafetyBackup_Click):
                Safety.CreateSafetyBackup_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Documents.DocumentThumbnail_Click):
                Documents.DocumentThumbnail_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Statistics.ExportStatisticsExcel_Click):
                Statistics.ExportStatisticsExcel_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Statistics.ExportStatisticsPdf_Click):
                Statistics.ExportStatisticsPdf_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Shell.Header_MouseLeftButtonDown):
                Shell.Header_MouseLeftButtonDown(sender!, (MouseButtonEventArgs)args);
                break;
            case nameof(Shell.MaximizeRestore_Click):
                Shell.MaximizeRestore_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Shell.Minimize_Click):
                Shell.Minimize_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Shell.NavAboutSafety_Click):
                Shell.NavAboutSafety_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Shell.NavApprovalDocument_Click):
                Shell.NavApprovalDocument_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Shell.NavArchive_Click):
                Shell.NavArchive_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Shell.NavDocumentsDates_Click):
                Shell.NavDocumentsDates_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Shell.NavFirmsOffers_Click):
                Shell.NavFirmsOffers_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Shell.NavGeneral_Click):
                Shell.NavGeneral_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Shell.NavHome_Click):
                Shell.NavHome_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Shell.NavInstitution_Click):
                Shell.NavInstitution_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Shell.NavSettings_Click):
                Shell.NavSettings_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Shell.NavStatistics_Click):
                Shell.NavStatistics_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Offers.OfferEditorDate_Changed):
                Offers.OfferEditorDate_Changed(sender!, (RoutedPropertyChangedEventArgs<DateTime?>)args);
                break;
            case nameof(Offers.OfferEditorText_Changed):
                Offers.OfferEditorText_Changed(sender!, (TextChangedEventArgs)args);
                break;
            case nameof(Offers.OfferFirmSelector_SelectionChanged):
                Offers.OfferFirmSelector_SelectionChanged(sender!, (SelectionChangedEventArgs)args);
                break;
            case nameof(Offers.OfferGrid_CellEditEnding):
                Offers.OfferGrid_CellEditEnding(sender!, (DataGridCellEditEndingEventArgs)args);
                break;
            case nameof(Safety.OpenBackupFolder_Click):
                Safety.OpenBackupFolder_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Safety.OpenDataFolder_Click):
                Safety.OpenDataFolder_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Safety.OpenLogFolder_Click):
                Safety.OpenLogFolder_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Documents.OutputGroupPdf_Click):
                Documents.OutputGroupPdf_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Catalog.ProductDelete_Click):
                Catalog.ProductDelete_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Catalog.ProductEdit_Click):
                Catalog.ProductEdit_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Statistics.RefreshStatistics_Click):
                Statistics.RefreshStatistics_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Safety.RestoreSafetyBackup_Click):
                Safety.RestoreSafetyBackup_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Catalog.RowDelete_Click):
                Catalog.RowDelete_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Catalog.RowEdit_Click):
                Catalog.RowEdit_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Safety.RunIntegrityCheck_Click):
                Safety.RunIntegrityCheck_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Institution.SaveApplicationSettings_Click):
                Institution.SaveApplicationSettings_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Documents.SaveDates_Click):
                Documents.SaveDates_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Offers.SaveOffer_Click):
                Offers.SaveOffer_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Workspace.Save_Click):
                Workspace.Save_Click(sender!, (RoutedEventArgs)args);
                break;
            case nameof(Statistics.StatisticsYear_SelectionChanged):
                Statistics.StatisticsYear_SelectionChanged(sender!, (SelectionChangedEventArgs)args);
                break;
            case nameof(Catalog.TransientFirmEditor_TextChanged):
                Catalog.TransientFirmEditor_TextChanged(sender!, (TextChangedEventArgs)args);
                break;
            case nameof(Catalog.TransientProductEditor_SelectionChanged):
                Catalog.TransientProductEditor_SelectionChanged(sender!, (SelectionChangedEventArgs)args);
                break;
            case nameof(Catalog.TransientProductEditor_TextChanged):
                Catalog.TransientProductEditor_TextChanged(sender!, (TextChangedEventArgs)args);
                break;
            case nameof(Shell.Window_Closing):
                Shell.Window_Closing(sender!, (CancelEventArgs)args);
                break;
            case nameof(Shell.Window_PreviewKeyDown):
                Shell.Window_PreviewKeyDown(sender!, (KeyEventArgs)args);
                break;
            case nameof(Shell.Window_StateChanged):
                Shell.Window_StateChanged(sender!, (EventArgs)args);
                break;
            default:
                throw new ArgumentException("Bilinmeyen arayüz işlemi: " + action, nameof(action));
        }
    }
}
