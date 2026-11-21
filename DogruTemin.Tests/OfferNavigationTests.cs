using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using DogruTemin.Core.Services;
using DogruTemin.Wpf;

namespace DogruTemin.Tests;

[CollectionDefinition("WPF navigation", DisableParallelization = true)]
public sealed class WpfNavigationGroup;

[Collection("WPF navigation")]
public sealed class OfferNavigationTests
{
    [Fact]
    public void FirmOffersNavigationSelectsExistingFirmAndLoadsProductRows()
    {
        Exception? failure = null;
        var tempRoot = Path.Combine(Path.GetTempPath(), "DogruTemin_OfferNavigation_" + Guid.NewGuid().ToString("N"));
        var thread = new Thread(() =>
        {
            App? app = null;
            MainWindow? window = null;
            try
            {
                app = new App();
                app.InitializeComponent();
                var startup = typeof(App).GetMethod("Application_Startup", BindingFlags.Instance | BindingFlags.NonPublic)!;
                app.Startup -= startup.CreateDelegate<StartupEventHandler>(app);
                var storage = new StorageService(tempRoot);
                window = new MainWindow(storage);
                window.Show();
                window.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                AtlasThemeAcceptance.Review(window);
                var firm = new DogruTemin.Wpf.Models.Firm { Id = "firm-1", Name = "Örnek Firma" };
                window.Firms.Add(firm);
                window.Products.Add(new DogruTemin.Wpf.Models.ProductItem { Id = "product-1", No = 1, Name = "Örnek Ürün", Quantity = "1", Unit = "Adet" });


                WorkspaceTestDriver.Invoke(window, "NavFirmsOffers_Click", new object[] { window, new RoutedEventArgs() });


                var selector = Assert.IsType<ComboBox>(window.FindName("OfferFirmSelector"));
                Assert.Same(firm, selector.SelectedItem);
                var itemTemplate = Assert.IsType<DataTemplate>(selector.ItemTemplate);
                var itemText = Assert.IsType<TextBlock>(itemTemplate.LoadContent());
                Assert.Equal("Name", System.Windows.Data.BindingOperations.GetBinding(itemText, TextBlock.TextProperty)?.Path.Path);
                Assert.Single(window.OfferRows);
                Assert.Equal("Örnek Ürün", window.OfferRows[0].ProductName);
                VerifyTenProductSaveAndResponsiveArchive(window, storage);
                PanelReviewAcceptance.Review(window);
                VerifyLiveDocumentPreviews(window);
                VerifyArchiveEditAndMonthlyDistribution(window, storage);
                VerifyPageActionsReachTheirOwningModules(window);
            }
            catch (Exception ex)
            {
                failure = ex;
            }
            finally
            {
                if (window is not null)
                {
                    window.Features.State.CloseApproved = true;
                    window.Close();
                }
                app?.Shutdown();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        try
        {
            if (failure is not null)
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
        }
        finally
        {
            if (Directory.Exists(tempRoot)) Directory.Delete(tempRoot, recursive: true);
        }
    }

    private static void VerifyPageActionsReachTheirOwningModules(MainWindow window)
    {
        Assert.Equal(9, window.Features.Modules.Count);
        Assert.Equal(9, window.Features.Modules.Select(module => module.GetType()).Distinct().Count());
        var box = Assert.IsType<TextBox>(window.FindName("ProductNameBox"));
        var previous = box.Text;
        var dirty = window.Session.ProductEditorDirty;
        box.Text = "Modül olay yönlendirme denemesi";
        Assert.True(window.Session.ProductEditorDirty); // The UserControl routes TextChanged to Catalog.
        box.Text = previous;
        window.Session.ProductEditorDirty = dirty;
        Assert.Throws<ArgumentException>(() => window.Features.HandleAction("unknown-action", null, EventArgs.Empty));
        WorkspaceTestDriver.Invoke(window, "RefreshSaveStatusVisual", null);
    }

    private static void VerifyLiveDocumentPreviews(MainWindow window)
    {
        void Invoke(string name, params object[] args) => WorkspaceTestDriver.Invoke(window, name, args);
        Assert.Null(window.FindName("ControlView"));
        Assert.Null(window.FindName("NavControl"));
        var submenu = (StackPanel)window.FindName("DataEntrySubmenu");
        var archive = (Button)window.FindName("NavArchive");
        Assert.Same(submenu, archive.Parent);
        var outputIndex = submenu.Children.OfType<Button>().ToList().FindIndex(button =>
            UiTree.Descendants<TextBlock>(button).Any(text => text.Text == "ÇIKTILAR"));
        Assert.Same(archive, submenu.Children[outputIndex + 1]);

        Invoke("ShowView", "Output");
        window.UpdateLayout();
        var output = Assert.IsType<Grid>(window.FindName("OutputView"));
        var cards = output.Children.OfType<HeaderedContentControl>().ToArray();
        Assert.Equal(2, cards.Length);
        var left = cards[0].TransformToAncestor(output).TransformBounds(new Rect(cards[0].RenderSize));
        var right = cards[1].TransformToAncestor(output).TransformBounds(new Rect(cards[1].RenderSize));
        Assert.InRange(Math.Abs(left.Top - right.Top), 0, 1);
        Assert.True(right.Left >= left.Right + 15, "Çıktı kartları yan yana yerleşmeli.");
        var last = window.Products.Last();
        window.Products.Remove(last);
        Invoke("RefreshDocumentPreviews");
        Assert.Equal(2, window.PreparationPreviews.Count);
        Assert.Equal(4, window.FinalPreviews.Count);
        Assert.All(window.PreparationPreviews.Concat(window.FinalPreviews), preview => Assert.NotNull(preview.Image));
        var preview = window.PreparationPreviews[0];
        Invoke("DocumentThumbnail_Click", new Button { Tag = preview }, new RoutedEventArgs());
        var popup = Assert.Single(window.OwnedWindows.OfType<DocumentPreviewWindow>());
        Assert.Same(preview, popup.DataContext);
        byte[] Pixels(System.Windows.Media.ImageSource image)
        {
            var bitmap = (System.Windows.Media.Imaging.BitmapSource)image;
            var pixels = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];
            bitmap.CopyPixels(pixels, bitmap.PixelWidth * 4, 0);
            return pixels;
        }
        var before = Pixels(preview.Image!);
        window.Products.Add(last);
        Invoke("RefreshDocumentPreviews");
        Assert.False(before.SequenceEqual(Pixels(preview.Image!)), "Yeni ürün minyatürde görünmüyor.");
        popup.UpdateLayout();
        Assert.Same(preview.Image, Assert.Single(UiTree.Descendants<Image>(popup)).Source);
        var data = (DogruTemin.Core.Models.ProcurementData)WorkspaceTestDriver.Invoke(window, "BuildPreviewData", null)!;
        Assert.Equal(10, data.Products.Count);
        popup.Close();
        var grid = (DataGrid)window.FindName("ProductGrid");
        Assert.Equal(DataGridGridLinesVisibility.All, grid.GridLinesVisibility);
        Assert.All(grid.Columns.OfType<DataGridTextColumn>(), column =>
        {
            var text = new TextBlock { Style = column.ElementStyle };
            Assert.Equal(TextAlignment.Center, text.TextAlignment);
        });
    }

    private static void VerifyArchiveEditAndMonthlyDistribution(MainWindow window, StorageService storage)
    {
        void Invoke(string name, params object[] args) => WorkspaceTestDriver.Invoke(window, name, args);
        Assert.Empty(UiTree.Descendants<Button>((DependencyObject)window.FindName("HomeView")));
        Invoke("SetSavedStatus", DateTime.Now);
        Invoke("RefreshArchiveRecords");
        var record = Assert.Single(window.ArchiveRecords);
        Invoke("ArchiveRowEdit_Click", new Button { DataContext = record }, new RoutedEventArgs(Button.ClickEvent));
        var save = (Button)window.FindName("ArchiveChangesSaveButton");
        Assert.False(save.IsEnabled);
        var job = (TextBox)window.FindName("JobTitleBox");
        job.Text += " • Arşiv düzenleme testi";
        Assert.True(save.IsEnabled);
        Invoke("Save_Click", window, new RoutedEventArgs());
        Assert.False(save.IsEnabled);
        Assert.Equal(job.Text, storage.LoadRecord(record.RecordId, out _)!.JobTitle);
        Assert.Equal(job.Text, Assert.Single(window.ArchiveRecords).JobTitle);
        Invoke("ShowView", "Archive");
        var grid = (DataGrid)window.FindName("ArchiveGrid");
        grid.SelectedIndex = 0;
        grid.ScrollIntoView(grid.SelectedItem);
        ((TextBox)window.FindName("ArchiveSearchBox")).Focus();
        window.UpdateLayout();
        var row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(0);
        Assert.All(UiTree.Descendants<DataGridCell>(row), cell =>
        {
            if (cell.Background is System.Windows.Media.SolidColorBrush brush)
                Assert.True(brush.Color.R < 180 || brush.Color.A == 0, "Seçili arşiv hücresi beyaz olmamalı.");
        });
        foreach (var month in new[] { 1, 1, 2 })
        {
            var data = ProcurementBoundaryTests.Scenario(1);
            data.Dates.ApprovalDate = new DateTime(2026, month, 15);
            storage.Save(data, false);
        }
        Invoke("RefreshStatisticsYears");
        ((ComboBox)window.FindName("StatisticsYearBox")).SelectedItem = "2026";
        Invoke("RefreshStatistics");
        Assert.Equal(2, ((Canvas)window.FindName("StatisticsDonutChart")).Children.Count);
        Assert.Equal("3", ((TextBlock)window.FindName("StatisticsDonutTotalText")).Text);
        Assert.Equal(2, ((System.Windows.Controls.WrapPanel)window.FindName("StatisticsDonutLegendPanel")).Children.Count);
    }

    private static void VerifyTenProductSaveAndResponsiveArchive(MainWindow window, StorageService storage)
    {
        void Invoke(string name, params object[] arguments) => WorkspaceTestDriver.Invoke(window, name, arguments);
        var data = ProcurementBoundaryTests.Scenario(10);
        foreach (var offer in data.Firms[0].Offers) { offer.ApproximateUnitPrice = 19.53m; offer.FinalUnitPrice = 19.53m; }
        storage.Save(data);
        window.Features.State.SuppressDirtyTracking = true;
        Invoke("ApplyDataToUi", data);
        window.Features.State.SuppressDirtyTracking = false;
        Invoke("SetSavedStatus", data.LastSavedAt!);
        Invoke("NavFirmsOffers_Click", window, new RoutedEventArgs());
        Assert.Equal(10, window.OfferRows.Count);
        Assert.Equal("10 / 10 ürün", ((TextBlock)window.FindName("ProductCountText")).Text);
        Assert.Equal("19,53", window.OfferRows[0].FinalPrice);
        window.OfferRows[0].FinalPrice = "19,5300";
        Invoke("SaveOffer_Click", window, new RoutedEventArgs());
        Assert.Equal(19.53m, storage.LoadRecord(data.RecordId, out _)!.Firms[0].Offers[0].FinalUnitPrice);
        Assert.Equal("19,53", window.OfferRows[0].FinalPrice);
        var status = (TextBlock)window.FindName("OfferEditorStatusTitle");
        var flags = $"Record={window.Session.RecordDirty}; Offer={window.Session.OfferEditorDirty}; Firm={window.Session.FirmEditorDirty}; Product={window.Session.ProductEditorDirty}";
        Assert.True(status.Text == "TEKLİF KALICI KAYDEDİLDİ", status.Text + " " + flags + " status=" + ((TextBlock)window.FindName("SaveStatusText")).Text);
        Invoke("RefreshArchiveRecords");
        Invoke("ShowView", "Archive");
        var grid = (DataGrid)window.FindName("ArchiveGrid");
        Assert.Equal(10, window.ArchiveRecords.Single().ProductCount);
        Assert.Contains(grid.Columns, c => (string)c.Header == "İŞLEMLER");
        var amountColumn = grid.Columns.Single(c => (string)c.Header == "NİHAİ TUTAR");
        var sortingRows = new System.Collections.ObjectModel.ObservableCollection<DogruTemin.Wpf.Models.ArchiveRecordRow>
        {
            new() { RecordId = "ten", FinalTotal = "10,00 TL", FinalTotalValue = 10m },
            new() { RecordId = "two", FinalTotal = "2,00 TL", FinalTotalValue = 2m }
        };
        var sortingView = System.Windows.Data.CollectionViewSource.GetDefaultView(sortingRows);
        sortingView.SortDescriptions.Add(new System.ComponentModel.SortDescription(amountColumn.SortMemberPath, System.ComponentModel.ListSortDirection.Ascending));
        Assert.Equal("two", sortingView.Cast<DogruTemin.Wpf.Models.ArchiveRecordRow>().First().RecordId);
        foreach (var size in new[] { new Size(1024, 600), new Size(1280, 720), new Size(1366, 768), new Size(1920, 1080) })
        {
            window.Width = size.Width;
            window.Height = size.Height;
            window.UpdateLayout();
            window.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            Assert.True(grid.ActualHeight > 0, $"Arşiv tablosu görünmüyor: {size}; height={grid.ActualHeight}; window={window.ActualWidth}x{window.ActualHeight}");
            var template = (DataGridTemplateColumn)grid.Columns.Last();
            var buttons = Assert.IsType<StackPanel>(template.CellTemplate.LoadContent()).Children.OfType<Button>().ToList();
            Assert.Equal(3, buttons.Count);
            grid.ScrollIntoView(grid.Items[0], template);
            window.UpdateLayout();
            window.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            window.UpdateLayout();
            var actionContent = template.GetCellContent(grid.Items[0]);
            Assert.NotNull(actionContent);
            var actionPanel = actionContent as StackPanel ?? UiTree.Descendants<StackPanel>(actionContent).First();
            var needed = actionPanel.Children.OfType<Button>().Sum(button => button.ActualWidth + button.Margin.Left + button.Margin.Right);
            Assert.True(needed <= actionContent.ActualWidth + 1, $"Satır işlemleri sığmıyor: {needed} / {actionContent.ActualWidth}; column={template.ActualWidth}; grid={grid.ActualWidth}x{grid.ActualHeight}; size={size}");
            foreach (var button in actionPanel.Children.OfType<Button>())
            {
                FrameworkElement caption = UiTree.Descendants<TextBlock>(button).FirstOrDefault()
                    ?? (FrameworkElement)UiTree.Descendants<System.Windows.Shapes.Path>(button).Single();
                var bounds = caption.TransformToAncestor(button).TransformBounds(new Rect(caption.RenderSize));
                Assert.True(bounds.Left >= 0 && bounds.Right <= button.ActualWidth + 1, $"Düğme yazısı kesiliyor: {button.Content}");
            }
            var root = (FrameworkElement)window.Content;
            Assert.True(root.ActualWidth <= window.ActualWidth + 1, $"Kök yerleşim pencereye sığmıyor: {root.ActualWidth} / {window.ActualWidth}");
            var acceptance = Environment.GetEnvironmentVariable("DOGRU_TEMIN_ACCEPTANCE_DIR");
            if (!string.IsNullOrWhiteSpace(acceptance))
            {
                Directory.CreateDirectory(acceptance);
                var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
                bitmap.Render(window);
                var png = new System.Windows.Media.Imaging.PngBitmapEncoder();
                png.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));
                using var stream = File.Create(Path.Combine(acceptance, $"archive-{size.Width}x{size.Height}.png"));
                png.Save(stream);
                var dpi = System.Windows.Media.VisualTreeHelper.GetDpi(window);
                File.WriteAllText(Path.Combine(acceptance, "native-dpi.txt"), $"Actual WPF scale: {dpi.DpiScaleX * 100:0}% x {dpi.DpiScaleY * 100:0}%; viewport sizes are window tests, not OS DPI changes.");
            }
        }
        var previewFolder = Environment.GetEnvironmentVariable("DOGRU_TEMIN_ACCEPTANCE_DIR");
        if (!string.IsNullOrWhiteSpace(previewFolder))
        {
            Invoke("ShowView", "Home");
            window.Width = 1366; window.Height = 768;
            window.UpdateLayout();
            var image = new System.Windows.Media.Imaging.RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
            image.Render(window);
            var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(image));
            using var file = File.Create(Path.Combine(previewFolder, "home-cards-1366x768.png"));
            encoder.Save(file);
        }
    }
}
