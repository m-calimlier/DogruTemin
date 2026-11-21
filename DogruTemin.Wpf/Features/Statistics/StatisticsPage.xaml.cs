using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;

namespace DogruTemin.Wpf.Features.Statistics;
public partial class StatisticsPage : UserControl
{
    public StatisticsPage() => InitializeComponent();
    private void ExportStatisticsExcel_Click(object sender, RoutedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Statistics.StatisticsModule.ExportStatisticsExcel_Click), sender, e);
    private void ExportStatisticsPdf_Click(object sender, RoutedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Statistics.StatisticsModule.ExportStatisticsPdf_Click), sender, e);
    private void RefreshStatistics_Click(object sender, RoutedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Statistics.StatisticsModule.RefreshStatistics_Click), sender, e);
    private void StatisticsYear_SelectionChanged(object sender, SelectionChangedEventArgs e) => WorkspaceActionRouter.Dispatch(this, nameof(global::DogruTemin.Wpf.Features.Statistics.StatisticsModule.StatisticsYear_SelectionChanged), sender, e);
}
