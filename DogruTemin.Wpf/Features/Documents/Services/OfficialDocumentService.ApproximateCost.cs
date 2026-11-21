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
    private static FixedPage BuildApproximateCostSheetPage(ProcurementData data)
    {
        var products = ActiveProducts(data);
        var firms = ActiveFirms(data);
        var calc = ProcurementEngine.Calculate(data);
        var compact = products.Count >= 6 || firms.Count >= 4;
        var page = NewPage(A4LandscapeWidth, A4LandscapeHeight);
        var root = NewPageRoot(page, A4LandscapeWidth, A4LandscapeHeight);
        var layout = new Grid();
        layout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.Children.Add(layout);
        var body = new StackPanel
        {
            Orientation = Orientation.Vertical
        };
        Grid.SetRow(body, 0);
        layout.Children.Add(body);
        body.Children.Add(DocumentTitle("YAKLAŞIK MALİYET HESAP CETVELİ", true, TextAlignment.Center, new Thickness(0, 0, 0, compact ? 8 : 12)));
        var info = new Grid
        {
            Margin = new Thickness(0, 0, 0, compact ? 8 : 12)
        };
        info.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(compact ? 120 : 135) });
        info.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        info.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(compact ? 135 : 150) });
        info.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(compact ? 125 : 145) });
        AddApproxInfoRow(info, 0, "İDARENİN ADI :", data.Institution.FullName, "Düzenleme Tarihi:", DateText(data.Dates.CostCalculationDate), compact);
        AddApproxInfoRow(info, 1, "İŞİN ADI :", data.JobTitle, string.Empty, string.Empty, compact);
        body.Children.Add(info);
        body.Children.Add(BuildApproximateCostTable(products, firms, calc, compact));
        body.Children.Add(T("Belgeyi Düzenleyenler;", false, TextAlignment.Left, new Thickness(0, compact ? 10 : 16, 0, compact ? 6 : 10)));
        body.Children.Add(BuildLabeledSignatureRow(data.MarketCommission, compact));
        var attachment = SmallText($"EK: {firms.Count} Adet Firma Teklifi", false, TextAlignment.Left, new Thickness(0, 0, 0, 0));
        attachment.HorizontalAlignment = HorizontalAlignment.Left;
        attachment.VerticalAlignment = VerticalAlignment.Bottom;
        Grid.SetRow(attachment, 1);
        layout.Children.Add(attachment);
        return page;
    }

    private static Grid BuildApproximateCostTable(List<ProductItem> products, List<Firm> firms, CalculationResult calc, bool compact)
    {
        const int firmSlots = 5;
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(38) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(compact ? 215 : 235) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(compact ? 62 : 68) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(compact ? 60 : 66) });
        var used = 38 + (compact ? 215 : 235) + (compact ? 62 : 68) + (compact ? 60 : 66) + (compact ? 98 : 108) + (compact ? 110 : 120);
        var remaining = A4LandscapeWidth - (2 * Margin) - used;
        var firmWidth = remaining / firmSlots;
        for (var i = 0; i < firmSlots; i++)
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(firmWidth) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(compact ? 98 : 108) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(compact ? 110 : 120) });
        // Excel örneğindeki iki katmanlı başlık: üst bantta firma adları, alt bantta sütun adları.
        AddRow(grid, compact ? 42 : 52);
        AddCell(grid, 0, 0, "FİRMA ADI", true, TextAlignment.Center, 4, 1);
        for (var f = 0; f < firmSlots; f++)
        {
            var name = f < firms.Count ? FirmDisplayName(firms[f]) : string.Empty;
            AddCell(grid, 0, f + 4, name, false, TextAlignment.Center, 1, 1);
        }

        AddCell(grid, 0, 4 + firmSlots, string.Empty, false);
        AddCell(grid, 0, 5 + firmSlots, string.Empty, false);
        AddRow(grid, compact ? 43 : 52);
        AddCell(grid, 1, 0, "S.No", true);
        AddCell(grid, 1, 1, "Mal / Hizmetin Adı", true);
        AddCell(grid, 1, 2, "Mal / Hizmetin\nMiktarı", true);
        AddCell(grid, 1, 3, "Ölçü\nBirimi", true);
        for (var f = 0; f < firmSlots; f++)
            AddCell(grid, 1, f + 4, "Mal/Hizmet\nBirim Fiyat Teklifi", true);
        AddCell(grid, 1, 4 + firmSlots, "Ortalama\nBirim Fiyat", true);
        AddCell(grid, 1, 5 + firmSlots, "Toplam\nMaliyet Bedeli", true);
        for (var i = 0; i < products.Count; i++)
        {
            AddRow(grid, compact ? 31 : 38);
            var p = products[i];
            var pc = calc.Products.FirstOrDefault(x => x.ProductId == p.Id);
            AddCell(grid, i + 2, 0, (i + 1).ToString(CultureInfo.InvariantCulture), true);
            AddCell(grid, i + 2, 1, ProductDisplay(p), false, TextAlignment.Left, 1, 1);
            AddCell(grid, i + 2, 2, p.Quantity, false);
            AddCell(grid, i + 2, 3, p.Unit, false);
            for (var f = 0; f < firmSlots; f++)
            {
                decimal? price = null;
                if (f < firms.Count)
                    price = firms[f].Offers.FirstOrDefault(o => o.ProductId == p.Id)?.ApproximateUnitPrice;
                AddCell(grid, i + 2, f + 4, Price(price), false);
            }

            AddCell(grid, i + 2, 4 + firmSlots, Price(pc?.ApproximateAverageUnitPrice), false);
            AddCell(grid, i + 2, 5 + firmSlots, Price(pc?.ApproximateLineTotal), false);
        }

        var totalRow = products.Count + 2;
        AddRow(grid, compact ? 26 : 31);
        AddCell(grid, totalRow, 0, string.Empty, false, TextAlignment.Center, 4 + firmSlots, 1);
        AddCell(grid, totalRow, 4 + firmSlots, "TOPLAM", true);
        AddCell(grid, totalRow, 5 + firmSlots, Price(calc.ApproximateCostTotal), true);
        return grid;
    }
}
