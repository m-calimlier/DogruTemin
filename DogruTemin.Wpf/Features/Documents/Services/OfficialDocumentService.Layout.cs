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
    private static void AddInstitutionHeader(StackPanel body, ProcurementData data, bool compact)
    {
        body.Children.Add(T("T.C.", false, TextAlignment.Center, null));
        var authorityText = data.Institution.TopAuthority.ToUpper(Tr);
        body.Children.Add(T(authorityText, false, TextAlignment.Center, null));
        body.Children.Add(T(data.Institution.ShortName, false, TextAlignment.Center, null));
    }

    private static StackPanel BuildInstitutionContactBlock(ProcurementData data, bool compact, Thickness margin)
    {
        var panel = new StackPanel
        {
            Margin = margin
        };
        panel.Children.Add(SmallText("Adres :", false, TextAlignment.Left, null));
        panel.Children.Add(SmallText(data.Institution.FullName, false, TextAlignment.Left, null));
        panel.Children.Add(SmallText(data.Institution.Address, false, TextAlignment.Left, null));
        panel.Children.Add(SmallText("Tel: " + data.Institution.Phone, false, TextAlignment.Left, null));
        panel.Children.Add(SmallText("E-Posta: " + data.Institution.Email, false, TextAlignment.Left, null));
        return panel;
    }

    private static Grid BuildSimpleSignatureRow(IEnumerable<CommissionMember> source, bool compact)
    {
        var members = source.Take(3).ToList();
        while (members.Count < 3)
            members.Add(new CommissionMember());
        var grid = new Grid
        {
            Margin = new Thickness(0, 0, 0, compact ? 4 : 8)
        };
        for (var i = 0; i < 3; i++)
            grid.ColumnDefinitions.Add(new ColumnDefinition());
        for (var i = 0; i < 3; i++)
        {
            var p = new StackPanel
            {
                Margin = new Thickness(8, 0, 8, 0)
            };
            p.Children.Add(T(members[i].Name, false, TextAlignment.Center, null));
            p.Children.Add(T(i == 0 ? "Komisyon Başkanı" : "Üye", false, TextAlignment.Center, null));
            Grid.SetColumn(p, i);
            grid.Children.Add(p);
        }

        return grid;
    }

    private static Grid BuildLabeledSignatureRow(IEnumerable<CommissionMember> source, bool compact)
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
        for (var i = 0; i < 3; i++)
        {
            var p = new StackPanel
            {
                Margin = new Thickness(8, 0, 8, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = compact ? 210 : 245
            };
            var nameLine = "Adı Soyadı : " + members[i].Name;
            p.Children.Add(T(nameLine, false, TextAlignment.Center));
            p.Children.Add(T("Unvanı: " + CommissionRole(i), false, TextAlignment.Center));
            p.Children.Add(T("İmzası :", false, TextAlignment.Center, new Thickness(0, compact ? 3 : 5, 0, 0)));
            Grid.SetColumn(p, i);
            grid.Children.Add(p);
        }

        return grid;
    }

    private static Grid NewOfferGrid(bool compact)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(42) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(compact ? 72 : 82) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(compact ? 72 : 82) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(compact ? 100 : 110) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(compact ? 100 : 110) });
        return grid;
    }

    private static void AddApproxInfoRow(Grid grid, int row, string label1, string value1, string label2, string value2, bool compact)
    {
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto, MinHeight = compact ? 24 : 28 });
        var l1 = T(label1, true);
        var v1 = T(value1);
        Grid.SetRow(l1, row);
        Grid.SetColumn(l1, 0);
        Grid.SetRow(v1, row);
        Grid.SetColumn(v1, 1);
        grid.Children.Add(l1);
        grid.Children.Add(v1);
        if (!string.IsNullOrWhiteSpace(label2))
        {
            var l2 = T(label2, true, TextAlignment.Right);
            var v2 = T(value2, false, TextAlignment.Center);
            Grid.SetRow(l2, row);
            Grid.SetColumn(l2, 2);
            Grid.SetRow(v2, row);
            Grid.SetColumn(v2, 3);
            grid.Children.Add(l2);
            grid.Children.Add(v2);
        }
    }

    private static void AddInfoRow(Grid grid, int row, string label, string value, double height)
    {
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto, MinHeight = height });
        var l = T(label, true, TextAlignment.Left);
        var v = T(value, false, TextAlignment.Left);
        Grid.SetRow(l, row);
        Grid.SetColumn(l, 0);
        Grid.SetRow(v, row);
        Grid.SetColumn(v, 1);
        grid.Children.Add(l);
        grid.Children.Add(v);
    }

    private static FixedPage NewPage(double width, double height) => new()
    {
        Width = width,
        Height = height,
        Background = Brushes.White
    };
    private static Grid NewPageRoot(FixedPage page, double width, double height)
    {
        var root = new Grid
        {
            Width = width - (2 * Margin),
            MinHeight = height - (2 * Margin),
            Background = Brushes.White
        };
        FixedPage.SetLeft(root, Margin);
        FixedPage.SetTop(root, Margin);
        page.Children.Add(root);
        return root;
    }

    private static TextBlock T(string? text, bool bold = false, TextAlignment align = TextAlignment.Left, Thickness? margin = null)
    {
        // WPF sizes use 96 units/inch; PDF point sizes use 72 units/inch.
        var block = new TextBlock
        {
            Text = text ?? string.Empty,
            FontFamily = new FontFamily(DocumentFont),
            FontSize = BodySize,
            LineHeight = BodySize * 1.2,
            LineStackingStrategy = LineStackingStrategy.BlockLineHeight,
            FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,
            TextAlignment = align,
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = margin ?? new Thickness(0),
            Foreground = Black
        };
        TextOptions.SetTextFormattingMode(block, TextFormattingMode.Ideal);
        TextOptions.SetTextHintingMode(block, TextHintingMode.Fixed);
        return block;
    }

    private static TextBlock DocumentTitle(string? text, bool bold = true, TextAlignment align = TextAlignment.Center, Thickness? margin = null)
    {
        var block = T(text, true, align, margin);
        block.FontSize = TitleSize;
        block.LineHeight = TitleSize * 1.2;
        return block;
    }

    private static TextBlock SmallText(string? text, bool bold = false, TextAlignment align = TextAlignment.Left, Thickness? margin = null)
    {
        var block = T(text, bold, align, margin);
        block.FontSize = SmallSize;
        block.LineHeight = SmallSize * 1.2;
        return block;
    }

    private static Border PlainText(string text, TextAlignment alignment, Thickness margin, double minHeight)
    {
        return new Border
        {
            Margin = margin,
            MinHeight = minHeight,
            VerticalAlignment = VerticalAlignment.Center,
            Child = T(text, false, alignment, null)
        };
    }

    private static void AddRow(Grid grid, double height) => grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(height) });
    private static void AddCell(Grid grid, int row, int col, string? text, bool bold, TextAlignment align = TextAlignment.Center, int colSpan = 1, int rowSpan = 1)
    {
        var border = new Border
        {
            BorderBrush = Black,
            BorderThickness = MediumBorder,
            Padding = new Thickness(4, 1.5, 4, 1.5),
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Child = T(text, bold, TextAlignment.Center, null)
        };
        ((TextBlock)border.Child).FontSize = bold && colSpan == grid.ColumnDefinitions.Count && colSpan <= 2 ? BodySize : TableSize;
        ((TextBlock)border.Child).LineHeight = ((TextBlock)border.Child).FontSize * 1.2;
        Grid.SetRow(border, row);
        Grid.SetColumn(border, col);
        if (colSpan > 1)
            Grid.SetColumnSpan(border, colSpan);
        if (rowSpan > 1)
            Grid.SetRowSpan(border, rowSpan);
        grid.Children.Add(border);
    }

    private static string CommissionRole(int index) => index == 0 ? "Komisyon Başkanı" : "Üye";
    private static List<ProductItem> ActiveProducts(ProcurementData data) => data.Products.Where(p => !string.IsNullOrWhiteSpace(p.Name)).Take(ProcurementLimits.MaxProducts).ToList();
    private static List<Firm> ActiveFirms(ProcurementData data) => data.Firms.Where(f => !string.IsNullOrWhiteSpace(f.Name)).Take(5).ToList();
    private static string ProductDisplay(ProductItem p)
    {
        if (string.IsNullOrWhiteSpace(p.Specification))
            return p.Name;
        if (p.Name.Contains(p.Specification, StringComparison.OrdinalIgnoreCase))
            return p.Name;
        return p.Name + "\n" + p.Specification;
    }

    private static string ProductInlineDisplay(ProductItem product) => ProductDisplay(product).Replace("\r", string.Empty).Replace("\n", " — ");
    private static string FirmDisplayName(Firm f)
    {
        if (string.IsNullOrWhiteSpace(f.ContactName))
            return f.Name;
        if (f.Name.Contains(f.ContactName, StringComparison.OrdinalIgnoreCase))
            return f.Name;
        return f.Name + "\n" + f.ContactName;
    }

    private static decimal? CompleteFinalOfferTotal(Firm firm, IReadOnlyCollection<ProductItem> products)
    {
        decimal total = 0m;
        foreach (var product in products)
        {
            if (!NumberParser.TryPositiveDecimal(product.Quantity, out var quantity))
                return null;
            var finalUnitPrice = firm.Offers.FirstOrDefault(o => o.ProductId == product.Id)?.FinalUnitPrice;
            if (!finalUnitPrice.HasValue || finalUnitPrice.Value <= 0m)
                return null;
            total += quantity * finalUnitPrice.Value;
        }

        return total > 0m ? total : null;
    }

    private static string MoneyNoSymbol(decimal? value) => value is> 0 ? value.Value.ToString("N2", Tr) : string.Empty;
    private static string MoneyNoSymbol(decimal value) => value > 0 ? value.ToString("N2", Tr) : string.Empty;
    private static string Price(decimal? value) => value is> 0 ? value.Value.ToString("N2", Tr) + " ₺" : string.Empty;
    private static string Price(decimal value) => value > 0 ? value.ToString("N2", Tr) + " ₺" : string.Empty;
    private static string DateText(DateTime? date) => date.HasValue ? date.Value.ToString("dd.MM.yyyy", Tr) : ".... / ..... / ........";
    private static string Safe(string? value) => string.IsNullOrWhiteSpace(value) ? "........................" : value.Trim();
    private static string ApprovalLine(ProcurementData data) => $": {DateText(data.Dates.ApprovalDate)} tarih ve HYS - {Safe(data.Dates.ApprovalNo)} sayılı Harcama Talimatı Onay Belgesi";
}
