using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using DogruTemin.Core.Models;
using DogruTemin.Wpf.Services;
using DogruTemin.Wpf;

namespace DogruTemin.Tests;

public class DocumentTypographyTests
{
    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    public void MarketApprovalRemainsCenteredAndRepeatedSignatureCaptionIsRemoved(int products)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                var data = ProcurementBoundaryTests.Scenario(products);
                data.Institution.AuthorizedName = "Deniz Örnek";
                data.Institution.AuthorizedTitle = "Harcama Yetkilisi";
                var document = OfficialDocumentService.BuildDocument(data, new[] { OutputDocumentType.MarketResearchReport });
                var page = (FixedPage)document.Pages[0].Child;
                var texts = UiTree.Descendants<TextBlock>(page).ToArray();
                Assert.All(texts, text => Assert.Equal("Times New Roman", text.FontFamily.Source));
                Assert.DoesNotContain(texts, text => text.Text == "Piyasa Fiyat Araştırması Görevlisi / Görevlileri");
                foreach (var text in texts.Where(text => text.Text is "UYGUNDUR" or "Deniz Örnek" or "Harcama Yetkilisi"))
                {
                    var bounds = text.TransformToAncestor(page).TransformBounds(new Rect(text.RenderSize));
                    Assert.InRange(Math.Abs(bounds.Left + bounds.Width / 2 - page.Width / 2), 0, 1);
                    Assert.Equal(16, text.FontSize, 2);
                    if (products == 5)
                    {
                        var unit = text.TransformToAncestor(page).TransformBounds(new Rect(0, 0, 1, 1));
                        Assert.Equal(12, text.FontSize * unit.Height * 72 / 96, 1);
                    }
                }
                var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".pdf");
                try
                {
                    PdfVectorWriter.Save(document, path);
                    if (products == 5)
                    {
                        using var pdf = UglyToad.PdfPig.PdfDocument.Open(path);
                        var approval = Assert.Single(pdf.GetPage(1).GetWords(), word => word.Text == "UYGUNDUR");
                        Assert.All(approval.Letters, letter => Assert.Equal(12, letter.FontSize, 1));
                    }
                }
                finally { File.Delete(path); }
            }
            catch (Exception ex) { failure = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(60)), "Belge yerleşimi tamamlanamadı.");
        if (failure is not null) throw failure;
    }
}
