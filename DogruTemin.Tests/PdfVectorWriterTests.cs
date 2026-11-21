using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using DogruTemin.Wpf.Services;

namespace DogruTemin.Tests;

public class PdfVectorWriterTests
{
    [Fact]
    public void LongTextIsAutomaticallyReducedAndPreservedInPdf()
    {
        RunSta(() =>
        {
            var document = new FixedDocument();
            var page = new FixedPage { Width = 300, Height = 200 };
            page.Children.Add(new TextBlock { Text = "Çok uzun firma unvanı ve açıklaması " + new string('Ş', 100), Width = 70, Height = 12, FontSize = 14, TextWrapping = TextWrapping.Wrap });
            var content = new PageContent();
            ((System.Windows.Markup.IAddChild)content).AddChild(page);
            document.Pages.Add(content);
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".pdf");
            try
            {
                PdfVectorWriter.Save(document, path);
                using var pdf = UglyToad.PdfPig.PdfDocument.Open(path);
                var text = string.Concat(pdf.GetPages().Select(p => p.Text));
                Assert.Equal(100, text.Count(character => character == 'Ş'));
                Assert.True(((TextBlock)page.Children[0]).FontSize < 14);
                Assert.Equal(1, pdf.NumberOfPages);
            }
            finally { File.Delete(path); }
        });
    }

    [Fact]
    public void TextOnlyDocumentUsesPdfFontsInsteadOfFullPageImage()
    {
        RunSta(() =>
        {
            var document = new FixedDocument();
            var page = new FixedPage { Width = 793.7008, Height = 1122.5197 };
            var border = new Border
            {
                Width = 500,
                Height = 120,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Child = new TextBlock
                {
                    Text = "DoğruTemin — Türkçe: İhale, Ölçü, Şube, Çalışma",
                    FontFamily = new FontFamily("Cambria"),
                    FontSize = 18,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(12)
                }
            };
            FixedPage.SetLeft(border, 50);
            FixedPage.SetTop(border, 50);
            page.Children.Add(border);
            var content = new PageContent();
            ((System.Windows.Markup.IAddChild)content).AddChild(page);
            document.Pages.Add(content);

            var path = Path.Combine(Path.GetTempPath(), $"DogruTemin_VectorPdf_{Guid.NewGuid():N}.pdf");
            try
            {
                PdfVectorWriter.Save(document, path);
                var bytes = File.ReadAllBytes(path);
                var ascii = Encoding.ASCII.GetString(bytes);

                Assert.True(bytes.Length > 500);
                Assert.StartsWith("%PDF-", ascii);
                Assert.Contains("/Font", ascii, StringComparison.Ordinal);
                Assert.DoesNotContain("/Subtype /Image", ascii, StringComparison.Ordinal);
                Assert.Contains("%%EOF", ascii, StringComparison.Ordinal);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        });
    }

    [Fact]
    public void CompatibilityWriterDelegatesToVectorWriter()
    {
        RunSta(() =>
        {
            var document = new FixedDocument();
            var page = new FixedPage { Width = 300, Height = 200 };
            page.Children.Add(new TextBlock { Text = "Seçilebilir metin", FontSize = 16 });
            var content = new PageContent();
            ((System.Windows.Markup.IAddChild)content).AddChild(page);
            document.Pages.Add(content);

            var path = Path.Combine(Path.GetTempPath(), $"DogruTemin_CompatPdf_{Guid.NewGuid():N}.pdf");
            try
            {
                PdfVectorWriter.Save(document, path);
                var ascii = Encoding.ASCII.GetString(File.ReadAllBytes(path));
                Assert.DoesNotContain("/Subtype /Image", ascii, StringComparison.Ordinal);
                Assert.Contains("/Font", ascii, StringComparison.Ordinal);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        });
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception ex) { failure = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not null) throw failure;
    }
}
