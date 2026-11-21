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
    private static FixedPage BuildFirmOfferFormPage(ProcurementData data)
    {
        var products = ActiveProducts(data);
        var compact = products.Count >= 6;
        var page = NewPage(A4PortraitWidth, A4PortraitHeight);
        var root = NewPageRoot(page, A4PortraitWidth, A4PortraitHeight);
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
        AddInstitutionHeader(body, data, compact);
        var meta = new Grid
        {
            Margin = new Thickness(0, compact ? 10 : 16, 0, compact ? 18 : 26)
        };
        meta.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(compact ? 190 : 220) });
        meta.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        meta.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto, MinHeight = compact ? 24 : 28 });
        meta.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto, MinHeight = compact ? 24 : 28 });
        var hbLabel = T("Harcama Belgesi Tarih ve Sayısı:", true, TextAlignment.Left, null);
        var hbText = $"{DateText(data.Dates.ApprovalDate)} tarih ve HYS - {Safe(data.Dates.ApprovalNo)} sayılı Harcama Talimatı Onay Belgesi";
        var hbValue = T(hbText, false, TextAlignment.Left, null);
        var konuLabel = T("Konu:", true, TextAlignment.Left, null);
        var konuValue = T("Teklifiniz", false, TextAlignment.Left, null);
        Grid.SetRow(hbLabel, 0);
        Grid.SetColumn(hbLabel, 0);
        Grid.SetRow(hbValue, 0);
        Grid.SetColumn(hbValue, 1);
        Grid.SetRow(konuLabel, 1);
        Grid.SetColumn(konuLabel, 0);
        Grid.SetRow(konuValue, 1);
        Grid.SetColumn(konuValue, 1);
        meta.Children.Add(hbLabel);
        meta.Children.Add(hbValue);
        meta.Children.Add(konuLabel);
        meta.Children.Add(konuValue);
        body.Children.Add(meta);
        body.Children.Add(T("İLGİLİ KURUMA / FİRMAYA", true, TextAlignment.Center, new Thickness(0, 0, 0, compact ? 2 : 4)));
        body.Children.Add(T(data.Institution.Province.ToUpper(Tr), false, TextAlignment.Right, new Thickness(0, 0, 0, compact ? 12 : 18)));
        var offerText = $"Kurumumuzun ihtiyacı olan aşağıda cinsi ve vasıfları belirtilen malzemeler 4734 sayılı Kamu İhale Kanununun 22/d maddesi gereğince doğrudan temin usulü ile alınacaktır. İlgilendiğiniz takdirde KDV hariç fiyat teklifinizi en geç {DocumentConstants.OfferDeadlinePlaceholder} tarihi mesai bitimine kadar göndermenizi, teklifinizde teslimat süresinin de bildirilmesini rica ederim.";
        body.Children.Add(PlainText(offerText, TextAlignment.Justify, new Thickness(0, 0, 0, compact ? 14 : 22), compact ? 78 : 98));
        body.Children.Add(BuildSimpleSignatureRow(data.MarketCommission, compact));
        body.Children.Add(BuildInstitutionContactBlock(data, compact, new Thickness(0, compact ? 16 : 24, 0, compact ? 10 : 14)));
        body.Children.Add(BuildFirmOfferItemsTable(products, compact));
        var accept = "Yukarıda belirtilen ve İdarenizce satın alınacak olan malların / hizmetlerin cinsi, özellikleri, miktarı ve diğer şartlarını okudum. KDV hariç toplam ........................................................... (..............................) Türk Lirası (TL) bedelle vermeyi kabul ve taahhüt ederim.";
        body.Children.Add(PlainText(accept, TextAlignment.Justify, new Thickness(0, compact ? 12 : 18, 0, compact ? 8 : 12), compact ? 58 : 74));
        var signer = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, compact ? 76 : 98, 5, compact ? 18 : 26)
        };
        signer.Children.Add(T("Teklif Eden", false, TextAlignment.Center, null));
        signer.Children.Add(T("Adı Soyadı - Ticaret Unvanı - İmza - Mühür", false, TextAlignment.Center, null));
        body.Children.Add(signer);
        var note = SmallText("NOT: Teklifiniz kapalı zarfta ve firma kaşesi zarfa basılı olarak teslim edilecektir.", true, TextAlignment.Left, new Thickness(0, 0, 0, 0));
        note.HorizontalAlignment = HorizontalAlignment.Left;
        note.VerticalAlignment = VerticalAlignment.Bottom;
        Grid.SetRow(note, 1);
        layout.Children.Add(note);
        return page;
    }

    private static Grid BuildFirmOfferItemsTable(List<ProductItem> products, bool compact)
    {
        var grid = NewOfferGrid(compact);
        AddRow(grid, compact ? 25 : 30);
        AddRow(grid, compact ? 38 : 46);
        AddCell(grid, 0, 0, "YAPILACAK İŞİN-HİZMETİN-MALIN VEYA MALZEMENİN", true, TextAlignment.Center, 4, 1);
        AddCell(grid, 0, 4, "Teklif Edilen (KDV Hariç)", true, TextAlignment.Center, 2, 1);
        AddCell(grid, 1, 0, "Sıra\nNo", true, TextAlignment.Center, 1, 1);
        AddCell(grid, 1, 1, "Cinsi/Özelliği", true, TextAlignment.Center, 1, 1);
        AddCell(grid, 1, 2, "Birimi", true, TextAlignment.Center, 1, 1);
        AddCell(grid, 1, 3, "Miktarı", true, TextAlignment.Center, 1, 1);
        AddCell(grid, 1, 4, "Birim Fiyatı", true, TextAlignment.Center, 1, 1);
        AddCell(grid, 1, 5, "Toplam Fiyatı", true, TextAlignment.Center, 1, 1);
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
        AddCell(grid, totalRow, 0, "KDV Hariç Teklif Edilen Toplam Fiyat:", false, TextAlignment.Right, 5, 1);
        AddCell(grid, totalRow, 5, string.Empty, false, TextAlignment.Center, 1, 1);
        AddRow(grid, compact ? 25 : 30);
        AddCell(grid, totalRow + 1, 0, "Yazıyla:", false, TextAlignment.Right, 2, 1);
        AddCell(grid, totalRow + 1, 2, string.Empty, false, TextAlignment.Left, 4, 1);
        return grid;
    }
}
