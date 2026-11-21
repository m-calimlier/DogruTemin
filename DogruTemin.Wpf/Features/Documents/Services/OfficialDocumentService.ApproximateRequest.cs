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
    private static FixedPage BuildApproximateRequestPage(ProcurementData data)
    {
        var products = ActiveProducts(data);
        var compact = products.Count >= 6;
        var page = NewPage(A4PortraitWidth, A4PortraitHeight);
        var root = NewPageRoot(page, A4PortraitWidth, A4PortraitHeight);
        var body = new StackPanel
        {
            Orientation = Orientation.Vertical
        };
        root.Children.Add(body);
        AddInstitutionHeader(body, data, compact);
        body.Children.Add(T("Alım ve Yetkilendirilen Görevlilere İlişkin", true, TextAlignment.Left, new Thickness(0, compact ? 10 : 16, 0, 2)));
        body.Children.Add(T($"Komisyon Görevlendirme Tarih ve Sayısı: {DateText(data.Dates.AssignmentDate)} tarih ve {Safe(data.Dates.AssignmentNo)} sayılı Müdürlük Makam Olur'u.", false, TextAlignment.Left, new Thickness(0, 0, 0, compact ? 18 : 28)));
        body.Children.Add(DocumentTitle("YAKLAŞIK MALİYET TESPİTİNE YÖNELİK PİYASA FİYAT ARAŞTIRMASI", true, TextAlignment.Center, new Thickness(0, 0, 0, compact ? 24 : 34)));
        body.Children.Add(T("İLGİLİ KİŞİ VEYA FİRMAYA", true, TextAlignment.Center, new Thickness(0, 0, 0, compact ? 16 : 22)));
        var requestText = "İdaremizce satın alınması / yaptırılması düşünülen aşağıda cinsi, miktarı, özellikleri ve diğer şartları yazılı mal, hizmet ya da yapım işlerinin 4734 sayılı Kamu İhale Kanununun 22/d maddesi gereği yaklaşık maliyetinin tespit edilmesinde değerlendirilmek üzere KDV hariç olmak üzere piyasada satış fiyatlarının bildirilmesini rica ederim.";
        body.Children.Add(PlainText(requestText, TextAlignment.Justify, new Thickness(0, 0, 0, compact ? 16 : 24), compact ? 74 : 94));
        body.Children.Add(BuildSimpleSignatureRow(data.MarketCommission, compact));
        body.Children.Add(BuildInstitutionContactBlock(data, compact, new Thickness(0, compact ? 16 : 24, 0, compact ? 10 : 14)));
        body.Children.Add(BuildApproximateRequestItemsTable(products, compact));
        body.Children.Add(SmallText("YALNIZ/KDV HARİÇ …………………………………………………………………………………………………", false, TextAlignment.Left, new Thickness(0, compact ? 12 : 18, 0, 0)));
        body.Children.Add(SmallText("NOT: Teklifler KDV hariç olarak verilecektir.", true, TextAlignment.Left, new Thickness(0, compact ? 18 : 28, 0, 0)));
        var stamp = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, compact ? 82 : 106, 10, 0)
        };
        stamp.Children.Add(T("Firma", false, TextAlignment.Center, null));
        stamp.Children.Add(T("İmza/Kaşe", false, TextAlignment.Center, null));
        body.Children.Add(stamp);
        return page;
    }

    private static Grid BuildApproximateRequestItemsTable(List<ProductItem> products, bool compact)
    {
        var grid = NewOfferGrid(compact);
        AddRow(grid, compact ? 25 : 30);
        AddRow(grid, compact ? 38 : 46);
        AddCell(grid, 0, 0, "YAPILACAK İŞİN-HİZMETİN-MALIN VEYA MALZEMENİN", true, TextAlignment.Center, 6, 1);
        AddCell(grid, 1, 0, "Sıra\nNo", true, TextAlignment.Center, 1, 1);
        AddCell(grid, 1, 1, "CİNSİ-ÖZELLİĞİ", true, TextAlignment.Center, 1, 1);
        AddCell(grid, 1, 2, "Birimi", true, TextAlignment.Center, 1, 1);
        AddCell(grid, 1, 3, "Miktarı", true, TextAlignment.Center, 1, 1);
        AddCell(grid, 1, 4, "Birim Fiyatı\nKDV HARİÇ", true, TextAlignment.Center, 1, 1);
        AddCell(grid, 1, 5, "TUTARI", true, TextAlignment.Center, 1, 1);
        for (var i = 0; i < products.Count; i++)
        {
            AddRow(grid, compact ? 27 : 34);
            var p = products[i];
            AddCell(grid, i + 2, 0, (i + 1).ToString(CultureInfo.InvariantCulture), false, TextAlignment.Center, 1, 1);
            AddCell(grid, i + 2, 1, ProductDisplay(p), false, TextAlignment.Left, 1, 1);
            AddCell(grid, i + 2, 2, p.Unit, false, TextAlignment.Center, 1, 1);
            AddCell(grid, i + 2, 3, p.Quantity, false, TextAlignment.Center, 1, 1);
            AddCell(grid, i + 2, 4, string.Empty, false, TextAlignment.Center, 1, 1);
            AddCell(grid, i + 2, 5, string.Empty, false, TextAlignment.Center, 1, 1);
        }

        var totalRow = products.Count + 2;
        AddRow(grid, compact ? 25 : 30);
        AddCell(grid, totalRow, 0, string.Empty, false, TextAlignment.Center, 4, 1);
        AddCell(grid, totalRow, 4, "TOPLAM", false, TextAlignment.Center, 1, 1);
        AddCell(grid, totalRow, 5, string.Empty, false, TextAlignment.Center, 1, 1);
        return grid;
    }
}
