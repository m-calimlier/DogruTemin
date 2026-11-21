using OutputDocumentType = DogruTemin.Core.Models.OutputDocumentType;
using DogruTemin.Wpf.Models;
using System.Collections.ObjectModel;
using AppSettings = DogruTemin.Core.Models.ApplicationSettings;

namespace DogruTemin.Wpf.Features;
internal sealed class WorkspaceState
{
    public ObservableCollection<Firm> Firms { get; } = new();
    public ObservableCollection<ProductItem> Products { get; } = new();
    public ObservableCollection<CommissionMember> InspectionCommission { get; } = new();
    public ObservableCollection<CommissionMember> MarketCommission { get; } = new();
    public ObservableCollection<OfferEditorRow> OfferRows { get; } = new();
    public ObservableCollection<ArchiveRecordRow> ArchiveRecords { get; } = new();
    public ObservableCollection<string> StatisticsYears { get; } = new();
    public ObservableCollection<MonthlyStatisticRow> MonthlyStatistics { get; } = new();
    public ObservableCollection<StatisticsOperationRow> StatisticsOperations { get; } = new();
    public ObservableCollection<string> CommissionTitles { get; } = new()
    {
        "Şube Müdürü",
        "Şef",
        "Memur",
        "Okul Müdürü",
        "Müdür Yardımcısı",
        "Öğretmen"
    };
    internal bool SuppressDirtyTracking { get; set; } = true;
    internal bool CloseApproved { get; set; }
    internal AppSettings Settings { get; set; } = new();
    public ObservableCollection<DocumentPreviewItem> PreparationPreviews { get; } = new()
    {
        new(OutputDocumentType.ApproximateRequest, "Yaklaşık Maliyet Talep Yazısı"),
        new(OutputDocumentType.FirmOfferForm, "Firma Teklif Formu")
    };
    public ObservableCollection<DocumentPreviewItem> FinalPreviews { get; } = new()
    {
        new(OutputDocumentType.ApproximateCostSheet, "Yaklaşık Maliyet Hesap Cetveli"),
        new(OutputDocumentType.ApprovalDocument, "Onay Belgesi"),
        new(OutputDocumentType.MarketResearchReport, "Piyasa Fiyat Araştırması Tutanağı"),
        new(OutputDocumentType.InspectionAcceptanceReport, "Muayene ve Kabul Tutanağı")
    };
}
