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
    public const double A4PortraitWidth = 793.7008;
    public const double A4PortraitHeight = 1122.5197;
    public const double A4LandscapeWidth = A4PortraitHeight;
    public const double A4LandscapeHeight = A4PortraitWidth;
    private const string DocumentFont = "Times New Roman";
    private const double BodySize = 12.0 * 96 / 72;
    private const double TitleSize = 14.0 * 96 / 72;
    private const double TableSize = 10.0 * 96 / 72;
    private const double SmallSize = 9.0 * 96 / 72;
    private const double SignatureSpace = 1.5 * 37.7952755906;
    private const double Cm = 37.7952755906;
    private const double Margin = 0.75 * Cm;
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");
    private static readonly Brush Black = Brushes.Black;
    private static readonly Thickness ThinBorder = new(0.75);
    private static readonly Thickness MediumBorder = new(1.05);
    public static FixedDocument BuildDocument(ProcurementData data, IEnumerable<OutputDocumentType> documentTypes)
    {
        ProcurementLimits.EnsureCapacity(data);
        var doc = new FixedDocument();
        foreach (var type in documentTypes)
        {
            FixedPage page = type switch
            {
                OutputDocumentType.ApproximateRequest => BuildApproximateRequestPage(data),
                OutputDocumentType.ApproximateCostSheet => BuildApproximateCostSheetPage(data),
                OutputDocumentType.ApprovalDocument => BuildApprovalDocumentPage(data),
                OutputDocumentType.FirmOfferForm => BuildFirmOfferFormPage(data),
                OutputDocumentType.MarketResearchReport => BuildMarketResearchPage(data),
                OutputDocumentType.InspectionAcceptanceReport => BuildInspectionAcceptancePage(data),
                _ => throw new NotSupportedException("Bu belge resmî belge motorunda uygulanmadı.")};
            DocumentTextFitter.Fit(page);
            var pc = new PageContent();
            ((System.Windows.Markup.IAddChild)pc).AddChild(page);
            doc.Pages.Add(pc);
        }

        return doc;
    }

    public static bool IsImplemented(OutputDocumentType type) => type is OutputDocumentType.ApproximateRequest or OutputDocumentType.ApproximateCostSheet or OutputDocumentType.ApprovalDocument or OutputDocumentType.FirmOfferForm or OutputDocumentType.MarketResearchReport or OutputDocumentType.InspectionAcceptanceReport;
}
