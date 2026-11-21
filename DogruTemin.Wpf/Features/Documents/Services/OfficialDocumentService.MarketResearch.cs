using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using DogruTemin.Core.Models;
using DogruTemin.Core.Services;

namespace DogruTemin.Wpf.Services;
public sealed partial class OfficialDocumentService
{
    // Eski Excel formu görsel referanstır; yeni iş kuralı ise alımın tamamında tek kazanan firmadır.
    private static FixedPage BuildMarketResearchPage(ProcurementData data)
    {
        var products = ActiveProducts(data);
        var firms = ActiveFirms(data);
        var calc = ProcurementEngine.Calculate(data);
        var winner = firms.FirstOrDefault(f => f.Id == calc.WinningFirmId);
        var compact = products.Count >= 6 || firms.Count >= 5;
        var dense = products.Count >= 7;
        var page = NewPage(A4LandscapeWidth, A4LandscapeHeight);
        var root = NewPageRoot(page, A4LandscapeWidth, A4LandscapeHeight);
        var body = new StackPanel
        {
            Orientation = Orientation.Vertical
        };
        root.Children.Add(body);
        body.Children.Add(DocumentTitle("PİYASA FİYAT ARAŞTIRMASI TUTANAĞI", true, TextAlignment.Center, new Thickness(0, 0, 0, dense ? 1 : compact ? 3 : 6)));
        var info = new Grid
        {
            Margin = new Thickness(0, 0, 0, dense ? 1 : compact ? 3 : 6)
        };
        info.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(320) });
        info.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        AddInfoRow(info, 0, "İdarenin Adı", data.Institution.FullName, 24);
        AddInfoRow(info, 1, "Yapılan İş / Mal / Hizmetin Adı, Niteliği", data.JobTitle, 24);
        AddInfoRow(info, 2, "Harcama Belgesi Tarih ve Sayısı", ApprovalLine(data), 24);
        body.Children.Add(info);
        body.Children.Add(BuildAllFirmOffersTable(products, firms, compact, dense));
        body.Children.Add(new Border { Height = dense ? 2 : compact ? 4 : 7, Background = Brushes.Transparent });
        body.Children.Add(BuildWinningFirmTable(products, winner, calc, compact, dense));
        var footer = new Border
        {
            BorderBrush = Black,
            BorderThickness = new Thickness(1.0, 0, 1.0, 1.0),
            Padding = new Thickness(dense ? 7 : compact ? 10 : 18, 4, dense ? 7 : compact ? 10 : 18, 4)
        };
        var footerPanel = new StackPanel();
        footer.Child = footerPanel;
        var reportDate = DateText(data.Dates.MarketReportDate);
        var legal = $"4734 sayılı Kamu İhale Kanununun 22 nci Maddesi uyarınca doğrudan temin usulüyle yapılacak alımlara ilişkin yapılan piyasa araştırmasında firmalarca/kişilerce teklif edilen fiyatlar tarafımca/tarafımızca değerlendirilerek yukarıda adı ve adresi belirtilen firmadan alım yapılması uygun görülmüştür. {reportDate}";
        footerPanel.Children.Add(PlainText(legal, TextAlignment.Justify, new Thickness(0, 0, 0, dense ? 1 : compact ? 2 : 5), 0));
        footerPanel.Children.Add(BuildMarketSignatureRow(data.MarketCommission, compact, dense));
        var closing = new Grid
        {
            Margin = new Thickness(0, 8, 0, 0)
        };
        closing.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        closing.ColumnDefinitions.Add(new ColumnDefinition());
        closing.ColumnDefinitions.Add(new ColumnDefinition());
        closing.Children.Add(SmallText("* Piyasa fiyat araştırması yapılacak kişi / firma, yer sayısına ihale yetkilisi karar verebilecektir.\n" + "* Piyasa fiyat araştırması için görevlendirilecek personelin sayısına ihale yetkilisi karar verebilecektir.", false, TextAlignment.Left, new Thickness(dense ? 6 : compact ? 10 : 22, dense ? 0 : 1, 0, dense ? 0 : compact ? 1 : 3)));
        var approval = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(12, 0, 12, 8)
        };
        Grid.SetColumn(approval, 1);
        approval.Children.Add(T("UYGUNDUR", false, TextAlignment.Center));
        approval.Children.Add(T(DateText(data.Dates.MarketReportDate), false, TextAlignment.Center));
        approval.Children.Add(new Border { Height = SignatureSpace });
        approval.Children.Add(T(data.Institution.AuthorizedName, false, TextAlignment.Center));
        if (!string.IsNullOrWhiteSpace(data.Institution.AuthorizedDuty))
            approval.Children.Add(T(data.Institution.AuthorizedDuty, false, TextAlignment.Center));
        if (!string.IsNullOrWhiteSpace(data.Institution.AuthorizedTitle))
            approval.Children.Add(T(data.Institution.AuthorizedTitle, false, TextAlignment.Center));
        closing.Children.Add(approval);
        footerPanel.Children.Add(closing);
        body.Children.Add(footer);
        return page;
    }

    private static Grid BuildAllFirmOffersTable(List<ProductItem> products, List<Firm> firms, bool compact, bool dense)
    {
        var firmSlots = Math.Max(3, firms.Count);
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(34) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(compact ? 390 : 420) });
        var used = 34 + (compact ? 390 : 420);
        var remaining = A4LandscapeWidth - (2 * Margin) - used;
        var firmWidth = remaining / firmSlots;
        for (var i = 0; i < firmSlots; i++)
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(firmWidth) });
        AddRow(grid, 22);
        AddRow(grid, 32);
        AddCell(grid, 0, 0, string.Empty, true, TextAlignment.Center, 1, 2);
        AddCell(grid, 0, 1, "Mal / Hizmet / Yapım İşi", true, TextAlignment.Center, 1, 2);
        AddCell(grid, 0, 2, "Kişi / Firmalar ve KDV Hariç Tüm Ürünler İçin Verilen Nihai Toplam Teklifleri", true, TextAlignment.Center, firmSlots, 1);
        for (var i = 0; i < firmSlots; i++)
        {
            var name = i < firms.Count ? FirmDisplayName(firms[i]) : string.Empty;
            AddCell(grid, 1, i + 2, name, false, TextAlignment.Center, 1, 1);
        }

        var rowHeight = dense ? 19 : 22;
        for (var r = 0; r < products.Count; r++)
        {
            AddRow(grid, rowHeight);
            var p = products[r];
            AddCell(grid, r + 2, 0, (r + 1).ToString(CultureInfo.InvariantCulture), false);
            AddCell(grid, r + 2, 1, ProductInlineDisplay(p), false, TextAlignment.Left, 1, 1);
        }

        var span = Math.Max(1, products.Count);
        for (var f = 0; f < firmSlots; f++)
        {
            var total = f < firms.Count ? Price(CompleteFinalOfferTotal(firms[f], products)) : string.Empty;
            AddCell(grid, 2, f + 2, total, false, TextAlignment.Center, 1, span);
        }

        return grid;
    }

    private static Grid BuildWinningFirmTable(List<ProductItem> products, Firm? winner, CalculationResult calc, bool compact, bool dense)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(34) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(compact ? 390 : 420) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(compact ? 180 : 195) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(compact ? 125 : 145) });
        AddRow(grid, 22);
        AddRow(grid, 26);
        AddCell(grid, 0, 0, string.Empty, true, TextAlignment.Center, 1, 2);
        AddCell(grid, 0, 1, "Mal / Hizmet / Yapım İşi", true, TextAlignment.Center, 1, 2);
        AddCell(grid, 0, 2, "Uygun Görülen Kişi / Firma / Firmalar", true, TextAlignment.Center, 3, 1);
        AddCell(grid, 1, 2, "Adı", true);
        AddCell(grid, 1, 3, "Adresi", true);
        AddCell(grid, 1, 4, "Teklif Ettiği Fiyat", true);
        var rowHeight = dense ? 19 : 22;
        for (var r = 0; r < products.Count; r++)
        {
            AddRow(grid, rowHeight);
            var p = products[r];
            AddCell(grid, r + 2, 0, (r + 1).ToString(CultureInfo.InvariantCulture), false);
            AddCell(grid, r + 2, 1, ProductInlineDisplay(p), false, TextAlignment.Left, 1, 1);
        }

        var span = Math.Max(1, products.Count);
        AddCell(grid, 2, 2, winner is null ? string.Empty : FirmDisplayName(winner), false, TextAlignment.Center, 1, span);
        AddCell(grid, 2, 3, winner?.Address ?? string.Empty, false, TextAlignment.Center, 1, span);
        AddCell(grid, 2, 4, Price(calc.MarketResultTotal), false, TextAlignment.Center, 1, span);
        var totalRow = products.Count + 2;
        AddRow(grid, 22);
        AddCell(grid, totalRow, 0, "TOPLAM (KDV HARİÇ):", true, TextAlignment.Right, 4, 1);
        AddCell(grid, totalRow, 4, Price(calc.MarketResultTotal), true);
        return grid;
    }

    private static Grid BuildMarketSignatureRow(IEnumerable<CommissionMember> source, bool compact, bool dense)
    {
        var members = source.Take(3).ToList();
        while (members.Count < 3)
            members.Add(new CommissionMember());
        var grid = new Grid
        {
            Margin = new Thickness(0, dense ? 0 : compact ? 0 : 1, 0, dense ? 0 : compact ? 0 : 2)
        };
        for (var i = 0; i < 3; i++)
            grid.ColumnDefinitions.Add(new ColumnDefinition());
        for (var i = 0; i < 3; i++)
        {
            var sp = new StackPanel
            {
                Margin = new Thickness(8, 0, 8, 0)
            };
            sp.Children.Add(T(members[i].Name, false, TextAlignment.Center));
            sp.Children.Add(T(CommissionRole(i), false, TextAlignment.Center, new Thickness(0, dense ? 0 : 1, 0, dense ? 0 : compact ? 2 : 5)));
            sp.Children.Add(T("........................................................", false, TextAlignment.Center));
            Grid.SetColumn(sp, i);
            grid.Children.Add(sp);
        }

        return grid;
    }
}
