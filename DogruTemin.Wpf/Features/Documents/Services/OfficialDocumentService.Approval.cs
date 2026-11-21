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
    private static FixedPage BuildApprovalDocumentPage(ProcurementData data)
    {
        var products = ActiveProducts(data);
        var calc = ProcurementEngine.Calculate(data);
        var info = data.ApprovalDocument ?? new ApprovalDocumentInfo();
        var page = NewPage(A4PortraitWidth, A4PortraitHeight);
        var root = NewPageRoot(page, A4PortraitWidth, A4PortraitHeight);
        var body = new StackPanel
        {
            Orientation = Orientation.Vertical
        };
        root.Children.Add(body);
        body.Children.Add(DocumentTitle("ONAY BELGESİ", true, TextAlignment.Center, new Thickness(0, 0, 0, 6)));
        var header = new Grid();
        AddApprovalSplitColumns(header);
        AddRow(header, 28);
        AddCell(header, 0, 0, "ALIM YAPAN İDARENİN ADI", true, TextAlignment.Left);
        AddCell(header, 0, 1, data.Institution.FullName, false, TextAlignment.Center, 1, 1);
        AddRow(header, 28);
        AddCell(header, 1, 0, "BELGE TARİH VE SAYISI", true, TextAlignment.Left);
        AddCell(header, 1, 1, $"{DateText(data.Dates.ApprovalDocumentDate)}  /  {Safe(info.DocumentNumber)}", false, TextAlignment.Center);
        body.Children.Add(header);
        var details = new Grid
        {
            Margin = new Thickness(0, 0, 0, 0)
        };
        AddApprovalSplitColumns(details);
        AddRow(details, 30);
        AddCell(details, 0, 0, "DOĞRUDAN TEMİN İLE İLGİLİ BİLGİLER", true, TextAlignment.Center, 2, 1);
        var row = 1;
        void Detail(string label, string value, double height = 27)
        {
            AddRow(details, height);
            AddCell(details, row, 0, label, true, TextAlignment.Left);
            AddCell(details, row, 1, value, false, TextAlignment.Left, 1, 1);
            row++;
        }

        Detail("İŞİN TANIMI", data.JobTitle, 36);
        Detail("İŞİN NİTELİĞİ", data.JobTitle, 36);
        Detail("İŞİN MİKTARI", ApprovalQuantitySummary(products), 34);
        Detail("Yaklaşık Maliyet (KDV Hariç)", NumberParser.Money(calc.ApproximateCostTotal));
        Detail("Kullanılabilir Ödenek Tutarı", ApprovalMoneyText(info.AvailableBudgetAmount));
        Detail("Yatırım Proje Numarası (varsa)", info.InvestmentProjectNumber);
        Detail("Bütçe Tertibi (varsa)", info.BudgetCode);
        Detail("Avans Verilecekse Şartları", info.AdvanceConditions);
        Detail("İhale Usulü", string.IsNullOrWhiteSpace(info.TenderProcedure) ? "4734 S.K. 22/d Kapsamında" : info.TenderProcedure);
        Detail("İlanın Şekli ve Adedi", string.IsNullOrWhiteSpace(info.AnnouncementMethod) ? "İlansız" : info.AnnouncementMethod);
        Detail("Ön Yeterlik/İhale Dokümanı Satış Bedeli", info.PrequalificationDocumentSalePrice);
        Detail("Fiyat Farkı Ödenecekse Dayanağı", string.IsNullOrWhiteSpace(info.PriceDifferenceBasis) ? "Ödenmeyecektir" : info.PriceDifferenceBasis, 32);
        body.Children.Add(details);
        var notes = new Grid();
        notes.ColumnDefinitions.Add(new ColumnDefinition());
        AddRow(notes, 28);
        AddCell(notes, 0, 0, "AÇIKLAMALAR", true, TextAlignment.Center);
        AddRow(notes, 55);
        AddCell(notes, 1, 0, string.IsNullOrWhiteSpace(info.Notes) ? data.JobTitle : info.Notes, false, TextAlignment.Left, 1, 1);
        body.Children.Add(notes);
        body.Children.Add(BuildApprovalCommissionTable("PİYASA ARAŞTIRMA GÖREVLİLERİ", data.MarketCommission));
        body.Children.Add(BuildApprovalCommissionTable("MUAYENE VE KABUL KOMİSYONU GÖREVLİLERİ", data.InspectionCommission));
        var approval = new Grid();
        AddApprovalSplitColumns(approval);
        AddRow(approval, 28);
        AddCell(approval, 0, 0, "ONAY", true, TextAlignment.Center, 2, 1);
        AddRow(approval, 172);
        var left = $"Yukarıda belirtilen mal/malzeme/hizmetin alınması hususunu onaylarınıza arz ederim\n{DateText(data.Dates.ApprovalDocumentDate)}";
        var rightTitle = !string.IsNullOrWhiteSpace(data.Institution.AuthorizedTitle) ? data.Institution.AuthorizedTitle : data.Institution.AuthorizedDuty;
        AddApprovalSigner(approval, 0, left, info.SubmittedByName, info.SubmittedByTitle);
        AddApprovalSigner(approval, 1, $"Uygundur\n{DateText(data.Dates.ApprovalDocumentDate)}", data.Institution.AuthorizedName, rightTitle);
        body.Children.Add(approval);
        return page;
    }

    private static void AddApprovalSigner(Grid grid, int column, string introduction, string name, string title)
    {
        var content = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8)
        };
        content.Children.Add(T(introduction, false, TextAlignment.Center));
        content.Children.Add(new Border { Height = SignatureSpace });
        content.Children.Add(T(name, false, TextAlignment.Center));
        content.Children.Add(T(title, false, TextAlignment.Center));
        var border = new Border
        {
            BorderBrush = Black,
            BorderThickness = MediumBorder,
            Child = content
        };
        Grid.SetRow(border, 1);
        Grid.SetColumn(border, column);
        grid.Children.Add(border);
    }

    // Onay Belgesi'nde ONAY bölümünün merkez çizgisi tüm iki sütunlu bloklar için tek referanstır.
    // Üst başlık, detaylar ve komisyon tabloları aynı %50 / %50 dikey aksı kullanır.
    private static void AddApprovalSplitColumns(Grid grid)
    {
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
    }

    private static Grid BuildApprovalCommissionTable(string title, IEnumerable<CommissionMember> source)
    {
        var members = source.Take(3).ToList();
        while (members.Count < 3)
            members.Add(new CommissionMember());
        var grid = new Grid();
        AddApprovalSplitColumns(grid);
        AddRow(grid, 28);
        AddCell(grid, 0, 0, title, true, TextAlignment.Center, 2, 1);
        AddRow(grid, 24);
        AddCell(grid, 1, 0, "Adı Soyadı", true);
        AddCell(grid, 1, 1, "Unvanı", true);
        for (var i = 0; i < 3; i++)
        {
            AddRow(grid, 25);
            AddCell(grid, i + 2, 0, $"{i + 1}. {members[i].Name}", false, TextAlignment.Left);
            AddCell(grid, i + 2, 1, $"{CommissionRole(i)} ({members[i].Title})", false, TextAlignment.Left);
        }

        return grid;
    }

    private static string ApprovalQuantitySummary(List<ProductItem> products) => products.Count == 0 ? string.Empty : string.Join("; ", products.Select(p => $"{p.Quantity} {p.Unit} {p.Name}"));
    private static string ApprovalMoneyText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;
        return NumberParser.TryPositiveDecimal(value, out var parsed) ? NumberParser.Money(parsed) : value.Trim();
    }
}
