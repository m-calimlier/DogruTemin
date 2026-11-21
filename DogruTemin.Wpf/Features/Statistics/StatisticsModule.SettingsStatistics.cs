using AppSettings = DogruTemin.Core.Models.ApplicationSettings;
using DogruTemin.Core.Services;
using DogruTemin.Wpf.Models;
using DogruTemin.Wpf.Services;
using Microsoft.Win32;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DogruTemin.Wpf.Features.Statistics;
internal sealed partial class StatisticsModule
{
    internal void RefreshStatisticsYears()
    {
        var statisticsYearBox = View<System.Windows.Controls.ComboBox>("StatisticsYearBox");
        if (statisticsYearBox is null)
            return;
        var selected = statisticsYearBox.SelectedItem?.ToString();
        var years = Context.Storage.ListRecords().Where(x => x.Dates.ApprovalDate.HasValue).Select(x => x.Dates.ApprovalDate!.Value.Year).Distinct().OrderByDescending(x => x).ToList();
        if (!years.Contains(DateTime.Now.Year))
            years.Add(DateTime.Now.Year);
        years = years.Distinct().OrderByDescending(x => x).ToList();
        Context.State.StatisticsYears.Clear();
        Context.State.StatisticsYears.Add("Tüm Yıllar");
        foreach (var year in years)
            Context.State.StatisticsYears.Add(year.ToString(CultureInfo.InvariantCulture));
        var target = !string.IsNullOrWhiteSpace(selected) && Context.State.StatisticsYears.Contains(selected) ? selected : DateTime.Now.Year.ToString(CultureInfo.InvariantCulture);
        statisticsYearBox.SelectedItem = Context.State.StatisticsYears.Contains(target) ? target : "Tüm Yıllar";
    }

    internal void RefreshStatistics_Click(object sender, RoutedEventArgs e)
    {
        RefreshStatisticsYears();
        RefreshStatistics();
    }

    internal void StatisticsYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!Context.State.SuppressDirtyTracking)
            RefreshStatistics();
    }

    internal void RefreshStatistics()
    {
        if (View<System.Windows.Controls.TextBlock>("StatisticsOperationCountText")is null || View<System.Windows.Controls.ComboBox>("StatisticsYearBox")is null)
            return;
        var allRecords = Context.Storage.ListRecords().ToList();
        var missingDateCount = allRecords.Count(x => !x.Dates.ApprovalDate.HasValue);
        var selectedText = View<System.Windows.Controls.ComboBox>("StatisticsYearBox").SelectedItem?.ToString() ?? DateTime.Now.Year.ToString(CultureInfo.InvariantCulture);
        int? selectedYear = int.TryParse(selectedText, out var parsedYear) ? parsedYear : null;
        var records = allRecords.Where(x => x.Dates.ApprovalDate.HasValue).Where(x => !selectedYear.HasValue || x.Dates.ApprovalDate!.Value.Year == selectedYear.Value).OrderBy(x => x.Dates.ApprovalDate).ToList();
        decimal approximateTotal = 0m, finalTotal = 0m, savingsTotal = 0m;
        var financialCompleteCount = 0;
        var operationRows = new List<StatisticsOperationRow>();
        var monthCounts = new int[13];
        var monthTotals = new decimal[13];
        foreach (var data in records)
        {
            var calc = ProcurementEngine.Calculate(data);
            var date = data.Dates.ApprovalDate!.Value;
            monthCounts[date.Month]++;
            if (calc.MarketResultTotal > 0m)
                monthTotals[date.Month] += calc.MarketResultTotal;
            if (calc.ApproximateCostTotal > 0m && calc.MarketResultTotal > 0m)
            {
                financialCompleteCount++;
                approximateTotal += calc.ApproximateCostTotal;
                finalTotal += calc.MarketResultTotal;
                var savings = calc.ApproximateCostTotal - calc.MarketResultTotal;
                savingsTotal += savings;
                operationRows.Add(new StatisticsOperationRow { RecordId = data.RecordId, JobTitle = string.IsNullOrWhiteSpace(data.JobTitle) ? "Başlıksız işlem" : data.JobTitle, Date = date, DateText = date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), ApproximateTotal = calc.ApproximateCostTotal, FinalTotal = calc.MarketResultTotal, Savings = savings, FinalText = NumberParser.Money(calc.MarketResultTotal), SavingsText = (savings >= 0m ? "+" : "") + NumberParser.Money(savings) });
            }
        }

        View<System.Windows.Controls.TextBlock>("StatisticsOperationCountText").Text = records.Count.ToString(CultureInfo.GetCultureInfo("tr-TR"));
        View<System.Windows.Controls.TextBlock>("StatisticsApproximateText").Text = approximateTotal > 0m ? NumberParser.Money(approximateTotal) : "—";
        View<System.Windows.Controls.TextBlock>("StatisticsFinalText").Text = finalTotal > 0m ? NumberParser.Money(finalTotal) : "—";
        View<System.Windows.Controls.TextBlock>("StatisticsSavingsText").Text = (approximateTotal > 0m || finalTotal > 0m) ? NumberParser.Money(savingsTotal) : "—";
        View<System.Windows.Controls.TextBlock>("StatisticsSavingsText").Foreground = savingsTotal < 0m ? new SolidColorBrush(Color.FromRgb(255, 165, 165)) : new SolidColorBrush(Color.FromRgb(126, 226, 166));
        var rate = approximateTotal > 0m ? (savingsTotal / approximateTotal) * 100m : 0m;
        View<System.Windows.Controls.TextBlock>("StatisticsSavingsRateText").Text = "%" + rate.ToString("N2", CultureInfo.GetCultureInfo("tr-TR"));
        View<System.Windows.Controls.TextBlock>("StatisticsAverageText").Text = financialCompleteCount > 0 ? NumberParser.Money(finalTotal / financialCompleteCount) : "—";
        View<System.Windows.Controls.TextBlock>("StatisticsExcludedText").Text = missingDateCount > 0 ? $"{missingDateCount} kayıt Harcama Belgesi tarihi olmadığı için dışarıda" : string.Empty;
        View<System.Windows.Controls.TextBlock>("StatisticsMonthlyHintText").Text = $"{financialCompleteCount} finansal kaydı tamamlanmış işlem";
        Context.State.MonthlyStatistics.Clear();
        var maxCount = Math.Max(1, monthCounts.Max());
        var tr = CultureInfo.GetCultureInfo("tr-TR");
        for (var month = 1; month <= 12; month++)
        {
            Context.State.MonthlyStatistics.Add(new MonthlyStatisticRow { MonthNumber = month, Month = tr.DateTimeFormat.GetAbbreviatedMonthName(month).TrimEnd('.').ToUpper(tr), Count = monthCounts[month], FinalTotal = monthTotals[month], TotalText = monthTotals[month] > 0m ? NumberParser.Money(monthTotals[month]) : "—", BarWidth = monthCounts[month] == 0 ? 0 : Math.Max(8, Math.Round(250.0 * monthCounts[month] / maxCount, 1)) });
        }

        RefreshMonthlyDonut();
        Context.State.StatisticsOperations.Clear();
        foreach (var row in operationRows.OrderByDescending(x => x.FinalTotal).ThenByDescending(x => x.Date).Take(10))
            Context.State.StatisticsOperations.Add(row);
        var highest = operationRows.OrderByDescending(x => x.FinalTotal).FirstOrDefault();
        var lowest = operationRows.OrderBy(x => x.FinalTotal).FirstOrDefault();
        View<System.Windows.Controls.TextBlock>("StatisticsHighestText").Text = highest is null ? "—" : $"{highest.JobTitle}  •  {highest.FinalText}";
        View<System.Windows.Controls.TextBlock>("StatisticsLowestText").Text = lowest is null ? "—" : $"{lowest.JobTitle}  •  {lowest.FinalText}";
    }

    internal int? SelectedStatisticsYear()
    {
        var selectedText = View<System.Windows.Controls.ComboBox>("StatisticsYearBox")?.SelectedItem?.ToString();
        return int.TryParse(selectedText, out var year) ? year : null;
    }

    internal ManagementReportSnapshot BuildCurrentManagementReport()
    {
        var institutionName = Context.State.Settings?.Institution?.FullName;
        return ManagementReportService.BuildSnapshot(Context.Storage.ListRecords(), SelectedStatisticsYear(), institutionName);
    }

    internal static string ReportPeriodFilePart(string period)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var value = new string (period.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
        return value.Replace(' ', '_');
    }

    internal void ExportStatisticsPdf_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var snapshot = BuildCurrentManagementReport();
            if (snapshot.IncludedOperationCount == 0)
            {
                MessageBox.Show("Seçili rapor döneminde Harcama Belgesi tarihi bulunan işlem yok.", "DoğruTemin - Yönetim Raporu", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var outputPath = WorkspaceOutputFile.CreatePath(Context.Storage, $"DogruTemin_Yonetim_Raporu_{ReportPeriodFilePart(snapshot.PeriodLabel)}.pdf", ".pdf");
            var document = ManagementReportService.BuildPdf(snapshot);
            PdfVectorWriter.Save(document, outputPath);
            Context.Workspace.UpdateHomeActivitySummary();
            UiNotificationService.Show(Context.Window, $"Yönetim raporu PDF olarak oluşturuldu:\n{outputPath}", TimeSpan.FromSeconds(5));
        }
        catch (Exception ex)
        {
            MessageBox.Show("PDF yönetim raporu oluşturulamadı: " + ex.Message, "DoğruTemin - Yönetim Raporu", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal void ExportStatisticsExcel_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var snapshot = BuildCurrentManagementReport();
            if (snapshot.IncludedOperationCount == 0)
            {
                MessageBox.Show("Seçili rapor döneminde Harcama Belgesi tarihi bulunan işlem yok.", "DoğruTemin - Yönetim Raporu", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var outputPath = WorkspaceOutputFile.CreatePath(Context.Storage, $"DogruTemin_Yonetim_Raporu_{ReportPeriodFilePart(snapshot.PeriodLabel)}.xlsx", ".xlsx");
            ManagementReportService.SaveExcel(snapshot, outputPath);
            UiNotificationService.Show(Context.Window, $"Yönetim raporu Excel olarak oluşturuldu:\n{outputPath}", TimeSpan.FromSeconds(5));
        }
        catch (Exception ex)
        {
            MessageBox.Show("Excel yönetim raporu oluşturulamadı: " + ex.Message, "DoğruTemin - Yönetim Raporu", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
