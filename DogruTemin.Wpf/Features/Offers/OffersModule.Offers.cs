using DogruTemin.Wpf.Models;
using FirmOfferCore = DogruTemin.Core.Models.FirmOffer;
using DogruTemin.Core.Services;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DogruTemin.Wpf.Features.Offers;
internal sealed partial class OffersModule
{
    internal void PrepareOffersView()
    {
        _suppressOfferEditorEvents = true;
        try
        {
            if (Context.State.Firms.Count == 0)
            {
                View<System.Windows.Controls.ComboBox>("OfferFirmSelector").SelectedItem = null;
                _offerEditingFirm = null;
                Context.State.OfferRows.Clear();
                View<System.Windows.Controls.TextBox>("OfferTaxOfficeBox").Text = string.Empty;
                View<DogruTemin.Wpf.LargeDatePicker>("OfferDatePicker").SelectedDate = null;
                View<System.Windows.Controls.TextBlock>("OfferSelectedTotalText").Text = "0,00 TL";
                SetOfferEditorBanner("FİRMA GEREKLİ", "Teklif girişi için önce en az bir firma ekleyin.", OfferBannerTone.Info);
                View<System.Windows.Controls.Button>("OfferSaveButton").IsEnabled = false;
                return;
            }

            if (Context.State.Products.Count == 0)
            {
                if (View<System.Windows.Controls.ComboBox>("OfferFirmSelector").SelectedItem is null)
                    View<System.Windows.Controls.ComboBox>("OfferFirmSelector").SelectedIndex = 0;
                _offerEditingFirm = View<System.Windows.Controls.ComboBox>("OfferFirmSelector").SelectedItem as Firm;
                Context.State.OfferRows.Clear();
                SetOfferEditorBanner("ÜRÜN GEREKLİ", "Teklif girişi için önce en az bir ürün ekleyin.", OfferBannerTone.Info);
                View<System.Windows.Controls.TextBlock>("OfferSelectedTotalText").Text = "0,00 TL";
                View<System.Windows.Controls.Button>("OfferSaveButton").IsEnabled = false;
                return;
            }

            View<System.Windows.Controls.Button>("OfferSaveButton").IsEnabled = true;
            var selected = View<System.Windows.Controls.ComboBox>("OfferFirmSelector").SelectedItem as Firm;
            if (selected is null || !Context.State.Firms.Contains(selected))
            {
                _suppressOfferSelection = true;
                View<System.Windows.Controls.ComboBox>("OfferFirmSelector").SelectedIndex = 0;
                _suppressOfferSelection = false;
                selected = View<System.Windows.Controls.ComboBox>("OfferFirmSelector").SelectedItem as Firm;
            }

            LoadOfferFirm(selected);
        }
        finally
        {
            _suppressOfferEditorEvents = false;
        }
    }

    internal void OfferFirmSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressOfferSelection || _suppressOfferEditorEvents)
            return;
        var next = View<System.Windows.Controls.ComboBox>("OfferFirmSelector").SelectedItem as Firm;
        if (Context.Session.OfferEditorDirty && _offerEditingFirm is not null)
        {
            if (!SaveOfferEditor(true))
            {
                _suppressOfferSelection = true;
                View<System.Windows.Controls.ComboBox>("OfferFirmSelector").SelectedItem = _offerEditingFirm;
                _suppressOfferSelection = false;
                return;
            }
        }

        LoadOfferFirm(next);
    }

    internal void LoadOfferFirm(Firm? firm)
    {
        _suppressOfferEditorEvents = true;
        try
        {
            _offerEditingFirm = firm;
            Context.Session.OfferEditorDirty = false;
            Context.State.OfferRows.Clear();
            if (firm is null)
            {
                View<System.Windows.Controls.TextBox>("OfferTaxOfficeBox").Text = string.Empty;
                View<DogruTemin.Wpf.LargeDatePicker>("OfferDatePicker").SelectedDate = null;
                View<System.Windows.Controls.TextBlock>("OfferSelectedTotalText").Text = "0,00 TL";
                SetOfferEditorBanner("FİRMA SEÇİN", "Tekliflerini düzenlemek istediğiniz firmayı seçin.", OfferBannerTone.Info);
                return;
            }

            View<System.Windows.Controls.TextBox>("OfferTaxOfficeBox").Text = firm.TaxOffice ?? string.Empty;
            View<DogruTemin.Wpf.LargeDatePicker>("OfferDatePicker").SelectedDate = firm.OfferDate;
            foreach (var p in Context.State.Products.Take(ProcurementLimits.MaxProducts))
            {
                var o = firm.Offers.FirstOrDefault(x => x.ProductId == p.Id);
                Context.State.OfferRows.Add(new OfferEditorRow { ProductId = p.Id, No = p.No, ProductName = p.Name, QuantityDisplay = string.IsNullOrWhiteSpace(p.Quantity) ? "—" : $"{p.Quantity} {p.Unit}", ApproximatePrice = o?.ApproximateUnitPrice?.ToString("0.##", CultureInfo.GetCultureInfo("tr-TR")) ?? string.Empty, FinalPrice = o?.FinalUnitPrice?.ToString("0.##", CultureInfo.GetCultureInfo("tr-TR")) ?? string.Empty });
            }

            RefreshOfferEditorSummary(firm);
        }
        finally
        {
            _suppressOfferEditorEvents = false;
        }
    }

    internal void OfferEditorText_Changed(object sender, TextChangedEventArgs e) => MarkOfferEditorDirty();
    internal void OfferEditorDate_Changed(object sender, RoutedPropertyChangedEventArgs<DateTime?> e) => MarkOfferEditorDirty();
    internal void OfferGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e) => MarkOfferEditorDirty();
    internal void MarkOfferEditorDirty()
    {
        if (_suppressOfferEditorEvents || _offerEditingFirm is null)
            return;
        Context.Session.OfferEditorDirty = true;
        Context.Documents.InvalidateDocumentPreviews();
        if (!Context.State.SuppressDirtyTracking)
            Context.Workspace.ShowDirtyStatus();
        var missingApprox = Context.State.OfferRows.Count(r => r.IsApproximatePriceMissing);
        var missingFinal = Context.State.OfferRows.Count(r => r.IsFinalPriceMissing);
        var parts = new List<string>();
        if (missingApprox > 0)
            parts.Add($"{missingApprox} yaklaşık fiyat eksik");
        if (missingFinal > 0)
            parts.Add($"{missingFinal} nihai fiyat eksik");
        var detail = parts.Count > 0 ? $"Kaydedilmemiş değişiklik var: {string.Join(", ", parts)}. Kırmızı alanları kontrol edin." : "Kaydedilmemiş değişiklik var. Tüm fiyat alanları dolu.";
        SetOfferEditorBanner("KAYDEDİLMEMİŞ DEĞİŞİKLİK", detail, OfferBannerTone.Dirty);
    }

    internal void SaveOffer_Click(object sender, RoutedEventArgs e)
    {
        View<System.Windows.Controls.DataGrid>("OfferGrid").CommitEdit(DataGridEditingUnit.Cell, true);
        View<System.Windows.Controls.DataGrid>("OfferGrid").CommitEdit(DataGridEditingUnit.Row, true);
        if (!SaveOfferEditor(true))
            return;
        Context.Workspace.Save_Click(Context.Window, new RoutedEventArgs());
        if (!Context.Workspace.HasUnsavedWork())
        {
            LoadOfferFirm(_offerEditingFirm);
            SetOfferEditorBanner("TEKLİF KALICI KAYDEDİLDİ", "Teklif ve işlem bilgileri diske kaydedildi.", OfferBannerTone.Success);
        }
        else
            SetOfferEditorBanner("KALICI KAYIT TAMAMLANMADI", "Teklif değişikliği çalışma alanında duruyor. Kaydı tamamlayın veya değişiklikten vazgeçin.", OfferBannerTone.Warning);
    }

    internal bool SaveOfferEditor(bool showErrors)
    {
        if (_offerEditingFirm is null)
            return true;
        if (Context.State.Products.Count == 0)
            return true;
        var offers = new List<FirmOfferCore>();
        foreach (var row in Context.State.OfferRows)
        {
            decimal? approx = null;
            decimal? final = null;
            if (!string.IsNullOrWhiteSpace(row.ApproximatePrice))
            {
                if (!NumberParser.TryPositiveDecimal(row.ApproximatePrice, out var value))
                {
                    if (showErrors)
                        MessageBox.Show($"{row.No}. ürün yaklaşık birim fiyatı 0'dan büyük, en fazla iki ondalık basamaklı bir sayı olmalıdır.", "DoğruTemin", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                approx = value;
            }

            if (!string.IsNullOrWhiteSpace(row.FinalPrice))
            {
                if (!NumberParser.TryPositiveDecimal(row.FinalPrice, out var value))
                {
                    if (showErrors)
                        MessageBox.Show($"{row.No}. ürün nihai birim fiyatı 0'dan büyük, en fazla iki ondalık basamaklı bir sayı olmalıdır.", "DoğruTemin", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                final = value;
            }

            offers.Add(new FirmOfferCore { ProductId = row.ProductId, ApproximateUnitPrice = approx, FinalUnitPrice = final });
        }

        _offerEditingFirm.TaxOffice = View<System.Windows.Controls.TextBox>("OfferTaxOfficeBox").Text.Trim();
        _offerEditingFirm.OfferDate = View<DogruTemin.Wpf.LargeDatePicker>("OfferDatePicker").SelectedDate;
        _offerEditingFirm.Offers = offers;
        Context.Session.PreferredWinnerFirmId = string.Empty;
        Context.Session.OfferEditorDirty = false;
        RefreshOfferEditorSummary(_offerEditingFirm, justSaved: true);
        Context.Workspace.MarkDirty();
        return true;
    }

    internal void RefreshOfferEditorSummary(Firm firm, bool justSaved = false)
    {
        decimal total = 0;
        var finalComplete = Context.State.Products.Count > 0;
        var missingFinal = 0;
        var missingApprox = 0;
        foreach (var p in Context.State.Products.Take(ProcurementLimits.MaxProducts))
        {
            var o = firm.Offers.FirstOrDefault(x => x.ProductId == p.Id);
            if (o?.ApproximateUnitPrice is not > 0)
                missingApprox++;
            if (o?.FinalUnitPrice is not > 0 || !NumberParser.TryPositiveDecimal(p.Quantity, out var qty))
            {
                finalComplete = false;
                missingFinal++;
                continue;
            }

            total += o.FinalUnitPrice.Value * qty;
        }

        View<System.Windows.Controls.TextBlock>("OfferSelectedTotalText").Text = NumberParser.Money(finalComplete ? total : 0m);
        var anyMissing = missingApprox > 0 || missingFinal > 0;
        if (!anyMissing)
        {
            SetOfferEditorBanner(justSaved ? "TEKLİF HAZIR • KAYIT BEKLİYOR" : "FİYATLAR TAMAM", justSaved ? $"{firm.Name}: teklif çalışma alanına alındı; işlem kaydı tamamlanana kadar değişiklik kalıcı değildir." : $"{firm.Name}: yaklaşık ve nihai fiyatların tamamı girildi.", OfferBannerTone.Success);
        }
        else
        {
            var details = new List<string>();
            if (missingApprox > 0)
                details.Add($"{missingApprox} yaklaşık fiyat eksik");
            if (missingFinal > 0)
                details.Add($"{missingFinal} nihai fiyat eksik");
            var consequence = missingFinal > 0 ? "Nihai fiyatı eksik firma değerlendirmeye alınmaz." : "Yaklaşık maliyet hesabı ve ilgili belge eksik kalır; firma nihai teklif değerlendirmesine katılabilir.";
            SetOfferEditorBanner(justSaved ? "KAYIT BEKLİYOR • EKSİK VAR" : "FİYAT EKSİK", $"{firm.Name}: {string.Join(", ", details)}. {consequence}" + (justSaved ? " Teklif çalışma alanında; kalıcı işlem kaydı henüz tamamlanmadı." : " Kırmızı işaretli alanları kontrol edin."), OfferBannerTone.Warning);
        }
    }

    private enum OfferBannerTone
    {
        Info,
        Dirty,
        Warning,
        Success
    }

    private void SetOfferEditorBanner(string title, string message, OfferBannerTone tone)
    {
        View<System.Windows.Controls.TextBlock>("OfferEditorStatusTitle").Text = title;
        View<System.Windows.Controls.TextBlock>("OfferEditorStatusText").Text = message;
        (Color background, Color border, Color iconBackground, Color titleColor, Color textColor, string icon) = tone switch
        {
            OfferBannerTone.Success => (Color.FromRgb(13, 28, 48), Color.FromRgb(67, 86, 109), Color.FromRgb(32, 56, 82), Color.FromRgb(143, 185, 194), Color.FromRgb(172, 187, 204), "✓"),
            OfferBannerTone.Warning => (Color.FromRgb(42, 35, 24), Color.FromRgb(143, 119, 65), Color.FromRgb(61, 48, 27), Color.FromRgb(223, 188, 112), Color.FromRgb(227, 211, 180), "!"),
            OfferBannerTone.Dirty => (Color.FromRgb(42, 35, 24), Color.FromRgb(143, 119, 65), Color.FromRgb(61, 48, 27), Color.FromRgb(223, 188, 112), Color.FromRgb(227, 211, 180), "!"),
            _ => (Color.FromRgb(13, 28, 48), Color.FromRgb(67, 86, 109), Color.FromRgb(32, 56, 82), Color.FromRgb(143, 185, 194), Color.FromRgb(172, 187, 204), "i")};
        View<System.Windows.Controls.Border>("OfferEditorStatusBanner").Background = new SolidColorBrush(background);
        View<System.Windows.Controls.Border>("OfferEditorStatusBanner").BorderBrush = new SolidColorBrush(border);
        View<System.Windows.Controls.Border>("OfferEditorStatusIconBorder").Background = new SolidColorBrush(iconBackground);
        View<System.Windows.Controls.TextBlock>("OfferEditorStatusTitle").Foreground = new SolidColorBrush(titleColor);
        View<System.Windows.Controls.TextBlock>("OfferEditorStatusText").Foreground = new SolidColorBrush(textColor);
        View<System.Windows.Controls.TextBlock>("OfferEditorStatusIcon").Foreground = new SolidColorBrush(titleColor);
        View<System.Windows.Controls.TextBlock>("OfferEditorStatusIcon").Text = icon;
    }
}
