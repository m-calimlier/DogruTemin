using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DogruTemin.Core.Models;
using DogruTemin.Core.Services;
using DogruTemin.Wpf.Models;
using DogruTemin.Wpf.Services;

namespace DogruTemin.Wpf.Features.Documents;
internal sealed partial class DocumentsModule
{
    public ObservableCollection<DocumentPreviewItem> PreparationPreviews { get; } = new()
    {
        new(OutputDocumentType.ApproximateRequest, "Yaklaşık Maliyet Talep Yazısı"),
        new(OutputDocumentType.FirmOfferForm, "Firma Teklif Formu")
    };
    public ObservableCollection<DocumentPreviewItem> FinalPreviews { get; } = new()
    {
        new(OutputDocumentType.ApproximateCostSheet, "Yaklaşık Maliyet Hesap Cetveli"),
        new(OutputDocumentType.ApprovalDocument, "Onay Belgesi"),
        new(OutputDocumentType.MarketResearchReport, "Piyasa Fiyat Araştırması Tutanağı"),
        new(OutputDocumentType.InspectionAcceptanceReport, "Muayene ve Kabul Tutanağı")
    };

    private DispatcherTimer? _previewTimer;
    private bool _previewsDirty = true;
    private readonly Dictionary<OutputDocumentType, DocumentPreviewWindow> _previewWindows = new();
    internal void InitializeDocumentPreviews()
    {
        _previewTimer = new DispatcherTimer(DispatcherPriority.Background, Context.Window.Dispatcher)
        {
            Interval = TimeSpan.FromMilliseconds(450)
        };
        _previewTimer.Tick += (_, _) =>
        {
            _previewTimer.Stop();
            RefreshDocumentPreviews();
        };
        foreach (var collection in new INotifyCollectionChanged[]
        {
            Context.State.Products,
            Context.State.Firms,
            Context.State.InspectionCommission,
            Context.State.MarketCommission,
            Context.State.OfferRows
        }

        )
            collection.CollectionChanged += (_, args) =>
            {
                if (args.OldItems is not null)
                    foreach (var item in args.OldItems.OfType<INotifyPropertyChanged>())
                        item.PropertyChanged -= PreviewSourceChanged;
                if (args.NewItems is not null)
                    foreach (var item in args.NewItems.OfType<INotifyPropertyChanged>())
                        item.PropertyChanged += PreviewSourceChanged;
                InvalidateDocumentPreviews();
            };
        Context.Window.Closed += (_, _) =>
        {
            _previewTimer.Stop();
            foreach (var popup in _previewWindows.Values.ToArray())
                popup.Close();
        };
    }

    internal void PreviewSourceChanged(object? sender, PropertyChangedEventArgs e) => InvalidateDocumentPreviews();
    internal void InvalidateDocumentPreviews()
    {
        _previewsDirty = true;
        if (_previewTimer is null || Context.State.SuppressDirtyTracking || !Context.Window.IsLoaded)
            return;
        _previewTimer.Stop();
        if (Context.Shell._currentView == "Output" || _previewWindows.Count > 0)
            _previewTimer.Start();
    }

    internal DogruTemin.Core.Models.ProcurementData BuildPreviewData()
    {
        var data = Context.Workspace.BuildDataFromUi();
        if (Context.Session.OfferEditorDirty && Context.Offers._offerEditingFirm is not null)
        {
            var firm = data.Firms.FirstOrDefault(f => f.Id == Context.Offers._offerEditingFirm.Id);
            if (firm is not null)
            {
                firm.OfferDate = View<DogruTemin.Wpf.LargeDatePicker>("OfferDatePicker").SelectedDate;
                firm.TaxOffice = View<System.Windows.Controls.TextBox>("OfferTaxOfficeBox").Text.Trim();
                foreach (var row in Context.State.OfferRows)
                {
                    var offer = firm.Offers.FirstOrDefault(o => o.ProductId == row.ProductId);
                    if (offer is null)
                    {
                        offer = new FirmOffer
                        {
                            ProductId = row.ProductId
                        };
                        firm.Offers.Add(offer);
                    }

                    offer.ApproximateUnitPrice = NumberParser.TryPositiveDecimal(row.ApproximatePrice, out var approximate) ? approximate : null;
                    offer.FinalUnitPrice = NumberParser.TryPositiveDecimal(row.FinalPrice, out var final) ? final : null;
                }
            }
        }

        return data;
    }

    internal void RefreshDocumentPreviews()
    {
        if (!_previewsDirty || Context.State.SuppressDirtyTracking)
            return;
        _previewTimer?.Stop();
        var data = BuildPreviewData();
        foreach (var item in Context.State.PreparationPreviews.Concat(Context.State.FinalPreviews))
        {
            try
            {
                var document = OfficialDocumentService.BuildDocument(data, new[] { item.Type });
                var page = (FixedPage)document.Pages[0].Child;
                var width = 1100;
                var height = (int)Math.Ceiling(width * page.Height / page.Width);
                var visual = new DrawingVisual();
                using (var context = visual.RenderOpen())
                {
                    context.DrawRectangle(Brushes.White, null, new Rect(0, 0, width, height));
                    context.DrawRectangle(new VisualBrush(page) { Stretch = Stretch.Fill }, null, new Rect(0, 0, width, height));
                }

                var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(visual);
                bitmap.Freeze();
                item.Update(bitmap, "Canlı önizleme • büyütmek için tıklayın");
            }
            catch (Exception error)
            {
                // Never retain an older image when the current draft cannot be rendered.
                item.Update(null, "Önizleme hazırlanamadı: " + error.Message);
            }
        }

        _previewsDirty = false;
    }

    internal void DocumentThumbnail_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: DocumentPreviewItem item })
            return;
        _previewsDirty = true;
        RefreshDocumentPreviews();
        if (_previewWindows.TryGetValue(item.Type, out var existing))
        {
            existing.Activate();
            return;
        }

        var popup = new DocumentPreviewWindow(item)
        {
            Owner = Context.Window
        };
        _previewWindows[item.Type] = popup;
        popup.Closed += (_, _) => _previewWindows.Remove(item.Type);
        popup.Show();
    }
}
