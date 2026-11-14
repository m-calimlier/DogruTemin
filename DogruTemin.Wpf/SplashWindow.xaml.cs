using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace DogruTemin.Wpf;

public partial class SplashWindow : Window
{
    private const double RingCenter = 60;
    private const double RingRadius = 54;
    private const int SegmentCount = 28;
    private int _reportedPercent;

    public SplashWindow()
    {
        InitializeComponent();
        Loaded += SplashWindow_Loaded;
    }

    private async void SplashWindow_Loaded(object sender, RoutedEventArgs e)
    {
        VersionText.Text = AppVersionInfo.DisplayVersion;
        ProgressTrack.Data = CreateRing(100);

        try
        {
            SetProgress(StartupProgressProfile.InitialPercent, "DoğruTemin başlatılıyor...");
            await Dispatcher.Yield(DispatcherPriority.Render);

            // MainWindow kurulumu UI iş parçacığını kısa süre meşgul edebildiği için önce
            // kullanıcıya gerçekten kesintisiz görünen bir ön yükleme akışı tamamlanır.
            await RunSmoothPreloadAsync();

            SetProgress(StartupProgressProfile.PreloadPercent, "Veriler ve uygulama bileşenleri hazırlanıyor...");
            var main = new MainWindow(SetProgress);

            CompleteProgress("Hazır. Program açılıyor...");
            await Task.Delay(StartupProgressProfile.CompletionDurationMs);

            Application.Current.MainWindow = main;
            main.Show();
            await Task.Delay(100);

            Close();
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }
        catch (Exception ex)
        {
            var path = App.WriteCrashLog(ex);
            MessageBox.Show(
                "DoğruTemin başlatılamadı. Teknik ayrıntılar güvenlik amacıyla ekranda gösterilmez.\n\nHata günlüğü:\n" + path,
                "DoğruTemin - Açılış Hatası",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Application.Current.Shutdown(-1);
        }
    }

    private async Task RunSmoothPreloadAsync()
    {
        var start = Environment.TickCount64;
        while (Environment.TickCount64 - start < StartupProgressProfile.PreloadDurationMs)
        {
            var elapsed = (Environment.TickCount64 - start) / (double)StartupProgressProfile.PreloadDurationMs;
            UpdateProgress(StartupProgressProfile.CalculatePreloadPercent(elapsed));
            await Task.Delay(StartupProgressProfile.FrameDelayMs);
        }
        UpdateProgress(StartupProgressProfile.PreloadPercent);
    }

    private void SetProgress(int percent, string stage)
    {
        StageText.Text = stage;
        UpdateProgress(Math.Clamp(percent, 0, StartupProgressProfile.MaximumReportedBeforeComplete));
        Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
    }

    private void UpdateProgress(int percent)
    {
        _reportedPercent = Math.Max(_reportedPercent, Math.Clamp(percent, 0, 100));
        PercentText.Text = $"%{_reportedPercent}";
        ProgressArc.Data = CreateRing(_reportedPercent);
        SetStages(_reportedPercent switch
        {
            < 34 => 0,
            < 68 => 1,
            < 100 => 2,
            _ => 3
        });
    }

    private void CompleteProgress(string stage)
    {
        StageText.Text = stage;
        UpdateProgress(100);
    }

    private static StreamGeometry CreateRing(int percent)
    {
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            var segmentAngle = 2 * Math.PI / SegmentCount;
            var progressAngle = percent * 2 * Math.PI / 100;
            for (var index = 0; index < SegmentCount; index++)
            {
                var start = index * segmentAngle + .013;
                if (start >= progressAngle) break;
                var end = Math.Min(progressAngle, (index + 1) * segmentAngle - .013);
                context.BeginFigure(PointOnRing(start), false, false);
                context.ArcTo(PointOnRing(end), new Size(RingRadius, RingRadius), 0, false, SweepDirection.Clockwise, true, false);
            }
        }
        geometry.Freeze();
        return geometry;
    }

    private static Point PointOnRing(double angle) =>
        new(RingCenter + RingRadius * Math.Sin(angle), RingCenter - RingRadius * Math.Cos(angle));

    private void SetStages(int activeStage)
    {
        var pending = (Brush)FindResource("StagePending");
        var done = (Brush)FindResource("StageDone");
        var active = (Brush)FindResource("StageActive");
        StageOne.Stroke = activeStage > 0 ? done : active;
        StageTwo.Stroke = activeStage == 1 ? active : activeStage > 1 ? done : pending;
        StageThree.Stroke = activeStage == 2 ? active : activeStage > 2 ? done : pending;
        StageOneCheck.Visibility = activeStage > 0 ? Visibility.Visible : Visibility.Collapsed;
        StageTwoCheck.Visibility = activeStage > 1 ? Visibility.Visible : Visibility.Collapsed;
        StageThreeCheck.Visibility = activeStage > 2 ? Visibility.Visible : Visibility.Collapsed;
        ActiveStageDot.Visibility = activeStage == 3 ? Visibility.Collapsed : Visibility.Visible;
        Canvas.SetTop(ActiveStageDot, 7.5 + 40 * activeStage);
    }

}
