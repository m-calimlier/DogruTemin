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
    private static FixedPage BuildInspectionAcceptancePage(ProcurementData data)
    {
        var products = ActiveProducts(data);
        var firms = ActiveFirms(data);
        var calc = ProcurementEngine.Calculate(data);
        var winner = firms.FirstOrDefault(f => f.Id == calc.WinningFirmId);
        var compact = products.Count >= 7;
        var page = NewPage(A4PortraitWidth, A4PortraitHeight);
        var root = NewPageRoot(page, A4PortraitWidth, A4PortraitHeight);
        var body = new StackPanel
        {
            Orientation = Orientation.Vertical
        };
        root.Children.Add(body);
        body.Children.Add(T("T.C.", false, TextAlignment.Center));
        var inspectionAuthority = data.Institution.TopAuthority.ToUpper(Tr);
        body.Children.Add(T(inspectionAuthority, false, TextAlignment.Center));
        body.Children.Add(T(data.Institution.ShortName, false, TextAlignment.Center));
        body.Children.Add(T(DateText(data.Dates.InspectionReportDate), false, TextAlignment.Right, new Thickness(0, compact ? 22 : 36, 0, compact ? 18 : 28)));
        body.Children.Add(DocumentTitle("TESLİM ALMA, MUAYENE VE KABUL TUTANAĞI", true, TextAlignment.Center, new Thickness(0, 0, 0, compact ? 18 : 28)));
        var winnerName = winner is null ? "........................................................" : FirmDisplayName(winner).Replace("\n", " ");
        var paragraph = $"{DateText(data.Dates.AssignmentDate)} tarih ve {Safe(data.Dates.AssignmentNo)} sayılı Müdürlük Makam Olur'u ve {DateText(data.Dates.ApprovalDate)} tarih ve HYS - {Safe(data.Dates.ApprovalNo)} sayılı Harcama Talimatı Onay Belgesi ile 4734 S.K. 22/d kapsamında {Safe(data.JobTitle)} doğrudan temin yoluyla satın alınmış olup, yüklenici firma \"{winnerName}\" tarafından, {DateText(data.Dates.DeliveryDate)} tarihinde eksiksiz olarak komisyonumuzca teslim alınmıştır.";
        body.Children.Add(PlainText(paragraph, TextAlignment.Justify, new Thickness(0, 0, 0, compact ? 18 : 28), compact ? 125 : 160));
        body.Children.Add(BuildInspectionItemsTable(products, compact));
        body.Children.Add(T("MUAYENE, TESLİM ALMA VE KABUL KOMİSYONU", true, TextAlignment.Center, new Thickness(0, compact ? 24 : 40, 0, compact ? 16 : 24)));
        body.Children.Add(BuildInspectionSignatureTable(data.InspectionCommission));
        return page;
    }

    private static Grid BuildInspectionItemsTable(List<ProductItem> products, bool compact)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(58) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        AddRow(grid, 38);
        AddCell(grid, 0, 0, "S.NO", true);
        AddCell(grid, 0, 1, "Cinsi/Özelliği", true);
        AddCell(grid, 0, 2, "BİRİMİ", true);
        AddCell(grid, 0, 3, "MİKTARI", true);
        for (var i = 0; i < products.Count; i++)
        {
            AddRow(grid, compact ? 37 : 48);
            var p = products[i];
            AddCell(grid, i + 1, 0, (i + 1).ToString(CultureInfo.InvariantCulture), true);
            AddCell(grid, i + 1, 1, ProductDisplay(p), false, TextAlignment.Center, 1, 1);
            AddCell(grid, i + 1, 2, p.Unit, false);
            AddCell(grid, i + 1, 3, p.Quantity, true);
        }

        return grid;
    }

    private static Grid BuildInspectionSignatureTable(IEnumerable<CommissionMember> source)
    {
        var members = source.Take(3).ToList();
        while (members.Count < 3)
            members.Add(new CommissionMember());
        var grid = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        for (var i = 0; i < 3; i++)
            grid.ColumnDefinitions.Add(new ColumnDefinition());
        AddRow(grid, 38);
        AddRow(grid, 34);
        for (var i = 0; i < 3; i++)
        {
            AddCell(grid, 0, i, members[i].Name, false, TextAlignment.Center, 1, 1);
            AddCell(grid, 1, i, CommissionRole(i), false);
        }

        foreach (var cell in grid.Children.OfType<Border>())
            if (cell.Child is TextBlock text)
            {
                text.FontSize = BodySize;
                text.LineHeight = BodySize * 1.2;
            }

        return grid;
    }
}
