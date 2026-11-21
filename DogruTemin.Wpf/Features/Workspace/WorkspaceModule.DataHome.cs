using DogruTemin.Wpf.Models;
using ProcurementData = DogruTemin.Core.Models.ProcurementData;
using OutputDocumentType = DogruTemin.Core.Models.OutputDocumentType;
using FirmOfferCore = DogruTemin.Core.Models.FirmOffer;
using DogruTemin.Core.Services;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace DogruTemin.Wpf.Features.Workspace;
internal sealed partial class WorkspaceModule
{
    internal void ApplyDataToUi(ProcurementData data)
    {
        ProcurementLimits.EnsureCapacity(data);
        Context.State.Firms.Clear();
        Context.State.Products.Clear();
        Context.State.InspectionCommission.Clear();
        Context.State.MarketCommission.Clear();
        Context.State.OfferRows.Clear();
        Context.Offers._offerEditingFirm = null;
        Context.Session.OfferEditorDirty = false;
        Context.Offers._suppressOfferSelection = true;
        View<System.Windows.Controls.ComboBox>("OfferFirmSelector").SelectedItem = null;
        Context.Offers._suppressOfferSelection = false;
        foreach (var p in data.Products.Take(ProcurementLimits.MaxProducts))
            Context.State.Products.Add(new ProductItem { Id = p.Id, No = p.No, Name = p.Name, Specification = p.Specification, Unit = p.Unit, Quantity = p.Quantity });
        foreach (var f in data.Firms.Take(5))
            Context.State.Firms.Add(new Firm { Id = f.Id, No = f.No, Name = f.Name, TaxOffice = f.TaxOffice, TaxNo = f.TaxNo, ContactName = f.ContactName, Phone = global::DogruTemin.Wpf.Features.Catalog.CatalogModule.PhoneForEntry(f.Phone), Email = f.Email, Address = f.Address, OfferDate = f.OfferDate, Offers = f.Offers.Select(o => new FirmOfferCore { ProductId = o.ProductId, ApproximateUnitPrice = o.ApproximateUnitPrice, FinalUnitPrice = o.FinalUnitPrice }).ToList() });
        foreach (var m in data.InspectionCommission.Take(3))
            Context.State.InspectionCommission.Add(new CommissionMember { No = m.No, Name = m.Name, Title = m.Title });
        foreach (var m in data.MarketCommission.Take(3))
            Context.State.MarketCommission.Add(new CommissionMember { No = m.No, Name = m.Name, Title = m.Title });
        View<System.Windows.Controls.TextBox>("JobTitleBox").Text = data.JobTitle;
        View<DogruTemin.Wpf.LargeDatePicker>("AssignmentDatePicker").SelectedDate = data.Dates.AssignmentDate;
        View<System.Windows.Controls.TextBox>("AssignmentNoBox").Text = data.Dates.AssignmentNo;
        View<DogruTemin.Wpf.LargeDatePicker>("ApprovalDatePicker").SelectedDate = data.Dates.ApprovalDate;
        View<System.Windows.Controls.TextBox>("ApprovalNoBox").Text = TextNormalizer.Hys(data.Dates.ApprovalNo);
        View<DogruTemin.Wpf.LargeDatePicker>("CostCalculationDatePicker").SelectedDate = data.Dates.CostCalculationDate;
        View<DogruTemin.Wpf.LargeDatePicker>("ApprovalDocumentDatePicker").SelectedDate = data.Dates.ApprovalDocumentDate;
        View<DogruTemin.Wpf.LargeDatePicker>("MarketReportDatePicker").SelectedDate = data.Dates.MarketReportDate ?? data.Dates.MarketApprovalDate;
        View<DogruTemin.Wpf.LargeDatePicker>("InspectionReportDatePicker").SelectedDate = data.Dates.InspectionReportDate;
        View<DogruTemin.Wpf.LargeDatePicker>("DeliveryDatePicker").SelectedDate = data.Dates.DeliveryDate;
        View<System.Windows.Controls.TextBox>("InstitutionTopAuthorityBox").Text = data.Institution.TopAuthority;
        View<System.Windows.Controls.TextBox>("InstitutionFullNameBox").Text = !string.IsNullOrWhiteSpace(data.Institution.FullName) ? data.Institution.FullName : data.Institution.ShortName;
        View<System.Windows.Controls.TextBox>("InstitutionProvinceBox").Text = data.Institution.Province;
        View<System.Windows.Controls.TextBox>("InstitutionAddressBox").Text = data.Institution.Address;
        View<System.Windows.Controls.TextBox>("InstitutionPhoneBox").Text = data.Institution.Phone;
        View<System.Windows.Controls.TextBox>("InstitutionEmailBox").Text = data.Institution.Email;
        View<System.Windows.Controls.TextBox>("AuthorizedNameBox").Text = data.Institution.AuthorizedName;
        View<System.Windows.Controls.TextBox>("AuthorizedDutyBox").Text = data.Institution.AuthorizedDuty;
        View<System.Windows.Controls.TextBox>("AuthorizedTitleBox").Text = data.Institution.AuthorizedTitle;
        View<System.Windows.Controls.TextBox>("ApprovalDocumentNumberBox").Text = data.ApprovalDocument.DocumentNumber;
        View<System.Windows.Controls.TextBox>("ApprovalAvailableBudgetBox").Text = data.ApprovalDocument.AvailableBudgetAmount;
        View<System.Windows.Controls.TextBox>("ApprovalInvestmentProjectBox").Text = data.ApprovalDocument.InvestmentProjectNumber;
        View<System.Windows.Controls.TextBox>("ApprovalBudgetCodeBox").Text = data.ApprovalDocument.BudgetCode;
        View<System.Windows.Controls.TextBox>("ApprovalAdvanceConditionsBox").Text = data.ApprovalDocument.AdvanceConditions;
        View<System.Windows.Controls.TextBox>("ApprovalTenderProcedureBox").Text = string.IsNullOrWhiteSpace(data.ApprovalDocument.TenderProcedure) ? "4734 S.K. 22/d Kapsamında" : data.ApprovalDocument.TenderProcedure;
        View<System.Windows.Controls.TextBox>("ApprovalAnnouncementMethodBox").Text = string.IsNullOrWhiteSpace(data.ApprovalDocument.AnnouncementMethod) ? "İlansız" : data.ApprovalDocument.AnnouncementMethod;
        View<System.Windows.Controls.TextBox>("ApprovalPrequalificationSalePriceBox").Text = data.ApprovalDocument.PrequalificationDocumentSalePrice;
        View<System.Windows.Controls.TextBox>("ApprovalPriceDifferenceBasisBox").Text = string.IsNullOrWhiteSpace(data.ApprovalDocument.PriceDifferenceBasis) ? "Ödenmeyecektir" : data.ApprovalDocument.PriceDifferenceBasis;
        View<System.Windows.Controls.TextBox>("ApprovalNotesBox").Text = data.ApprovalDocument.Notes;
        View<System.Windows.Controls.TextBox>("ApprovalSubmittedByNameBox").Text = data.ApprovalDocument.SubmittedByName;
        View<System.Windows.Controls.TextBox>("ApprovalSubmittedByTitleBox").Text = data.ApprovalDocument.SubmittedByTitle;
        View<System.Windows.Controls.CheckBox>("AutoBackupCheckBox").IsChecked = data.AutoBackup;
        Context.Session.LoadMetadata(data);
        Context.Catalog.ReNumberFirms();
        Context.Catalog.ReNumberProducts();
        View<System.Windows.Controls.TextBlock>("ProductCountText").Text = $"{Context.State.Products.Count} / {ProcurementLimits.MaxProducts} ürün";
    }

    internal ProcurementData BuildDataFromUi()
    {
        return new ProcurementData
        {
            RecordId = Context.Session.RecordId,
            Revision = Context.Session.Revision,
            LastOutput = Context.Session.LastOutput,
            CreatedAt = Context.Session.CreatedAt,
            OperationNumber = Context.Session.OperationNumber,
            JobTitle = View<System.Windows.Controls.TextBox>("JobTitleBox").Text.Trim(),
            Products = Context.State.Products.Select(p => new DogruTemin.Core.Models.ProductItem { Id = p.Id, No = p.No, Name = p.Name.Trim(), Specification = p.Specification.Trim(), Unit = p.Unit, Quantity = p.Quantity.Trim() }).ToList(),
            Firms = Context.State.Firms.Select(f => new DogruTemin.Core.Models.Firm { Id = f.Id, No = f.No, Name = f.Name.Trim(), TaxOffice = f.TaxOffice.Trim(), TaxNo = f.TaxNo.Trim(), ContactName = f.ContactName.Trim(), Phone = global::DogruTemin.Wpf.Features.Catalog.CatalogModule.PhoneForStorage(f.Phone), Email = f.Email.Trim().ToLowerInvariant(), Address = f.Address.Trim(), OfferDate = f.OfferDate, Offers = f.Offers.Select(o => new FirmOfferCore { ProductId = o.ProductId, ApproximateUnitPrice = o.ApproximateUnitPrice, FinalUnitPrice = o.FinalUnitPrice }).ToList() }).ToList(),
            InspectionCommission = Context.State.InspectionCommission.Take(3).Select(m => new DogruTemin.Core.Models.CommissionMember { No = m.No, Name = m.Name.Trim(), Title = m.Title.Trim() }).ToList(),
            MarketCommission = Context.State.MarketCommission.Take(3).Select(m => new DogruTemin.Core.Models.CommissionMember { No = m.No, Name = m.Name.Trim(), Title = m.Title.Trim() }).ToList(),
            Dates = new DogruTemin.Core.Models.ProcessDates
            {
                AssignmentDate = View<DogruTemin.Wpf.LargeDatePicker>("AssignmentDatePicker").SelectedDate,
                AssignmentNo = View<System.Windows.Controls.TextBox>("AssignmentNoBox").Text.Trim(),
                ApprovalDate = View<DogruTemin.Wpf.LargeDatePicker>("ApprovalDatePicker").SelectedDate,
                ApprovalNo = TextNormalizer.Hys(View<System.Windows.Controls.TextBox>("ApprovalNoBox").Text),
                CostCalculationDate = View<DogruTemin.Wpf.LargeDatePicker>("CostCalculationDatePicker").SelectedDate,
                ApprovalDocumentDate = View<DogruTemin.Wpf.LargeDatePicker>("ApprovalDocumentDatePicker").SelectedDate,
                MarketReportDate = View<DogruTemin.Wpf.LargeDatePicker>("MarketReportDatePicker").SelectedDate,
                InspectionReportDate = View<DogruTemin.Wpf.LargeDatePicker>("InspectionReportDatePicker").SelectedDate,
                DeliveryDate = View<DogruTemin.Wpf.LargeDatePicker>("DeliveryDatePicker").SelectedDate,
                MarketApprovalDate = View<DogruTemin.Wpf.LargeDatePicker>("MarketReportDatePicker").SelectedDate,
                OfferDeadlineDate = null
            },
            Institution = new DogruTemin.Core.Models.InstitutionInfo
            {
                TopAuthority = View<System.Windows.Controls.TextBox>("InstitutionTopAuthorityBox").Text.Trim(),
                ShortName = View<System.Windows.Controls.TextBox>("InstitutionFullNameBox").Text.Trim(),
                FullName = View<System.Windows.Controls.TextBox>("InstitutionFullNameBox").Text.Trim(),
                Province = View<System.Windows.Controls.TextBox>("InstitutionProvinceBox").Text.Trim(),
                Address = View<System.Windows.Controls.TextBox>("InstitutionAddressBox").Text.Trim(),
                Phone = View<System.Windows.Controls.TextBox>("InstitutionPhoneBox").Text.Trim(),
                Email = View<System.Windows.Controls.TextBox>("InstitutionEmailBox").Text.Trim().ToLowerInvariant(),
                AuthorizedName = View<System.Windows.Controls.TextBox>("AuthorizedNameBox").Text.Trim(),
                AuthorizedDuty = View<System.Windows.Controls.TextBox>("AuthorizedDutyBox").Text.Trim(),
                AuthorizedTitle = View<System.Windows.Controls.TextBox>("AuthorizedTitleBox").Text.Trim()
            },
            ApprovalDocument = new DogruTemin.Core.Models.ApprovalDocumentInfo
            {
                DocumentNumber = View<System.Windows.Controls.TextBox>("ApprovalDocumentNumberBox").Text.Trim(),
                AvailableBudgetAmount = View<System.Windows.Controls.TextBox>("ApprovalAvailableBudgetBox").Text.Trim(),
                InvestmentProjectNumber = View<System.Windows.Controls.TextBox>("ApprovalInvestmentProjectBox").Text.Trim(),
                BudgetCode = View<System.Windows.Controls.TextBox>("ApprovalBudgetCodeBox").Text.Trim(),
                AdvanceConditions = View<System.Windows.Controls.TextBox>("ApprovalAdvanceConditionsBox").Text.Trim(),
                TenderProcedure = View<System.Windows.Controls.TextBox>("ApprovalTenderProcedureBox").Text.Trim(),
                AnnouncementMethod = View<System.Windows.Controls.TextBox>("ApprovalAnnouncementMethodBox").Text.Trim(),
                PrequalificationDocumentSalePrice = View<System.Windows.Controls.TextBox>("ApprovalPrequalificationSalePriceBox").Text.Trim(),
                PriceDifferenceBasis = View<System.Windows.Controls.TextBox>("ApprovalPriceDifferenceBasisBox").Text.Trim(),
                Notes = View<System.Windows.Controls.TextBox>("ApprovalNotesBox").Text.Trim(),
                SubmittedByName = View<System.Windows.Controls.TextBox>("ApprovalSubmittedByNameBox").Text.Trim(),
                SubmittedByTitle = View<System.Windows.Controls.TextBox>("ApprovalSubmittedByTitleBox").Text.Trim()
            },
            PreferredWinnerFirmId = Context.Session.PreferredWinnerFirmId,
            AutoBackup = View<System.Windows.Controls.CheckBox>("AutoBackupCheckBox").IsChecked == true
        };
    }

    internal void UpdateHomeProcessMap(ProcurementData data)
    {
        var products = data.Products.Where(p => !string.IsNullOrWhiteSpace(p.Name)).Take(ProcurementLimits.MaxProducts).ToList();
        var firms = data.Firms.Where(f => !string.IsNullOrWhiteSpace(f.Name)).Take(5).ToList();
        var institutionDone = !string.IsNullOrWhiteSpace(data.Institution.TopAuthority) && !string.IsNullOrWhiteSpace(data.Institution.FullName) && !string.IsNullOrWhiteSpace(data.Institution.Province) && !string.IsNullOrWhiteSpace(data.Institution.Address) && ValidationService.ValidPhone(data.Institution.Phone) && ValidationService.ValidEmail(data.Institution.Email);
        var inspectionCommissionDone = data.InspectionCommission.Take(3).Count(m => !string.IsNullOrWhiteSpace(m.Name) && !string.IsNullOrWhiteSpace(m.Title)) == 3;
        var marketCommissionDone = data.MarketCommission.Take(3).Count(m => !string.IsNullOrWhiteSpace(m.Name) && !string.IsNullOrWhiteSpace(m.Title)) == 3;
        var commissionDone = inspectionCommissionDone && marketCommissionDone;
        var institutionCommissionDone = institutionDone && commissionDone;
        SetHomeCombinedStage(View<System.Windows.Controls.TextBlock>("HomeStageCommissionStatus"), View<System.Windows.Controls.TextBlock>("HomeStageCommissionDetail"), institutionDone, commissionDone, "Kurum bilgisi ve iki komisyon hazır.", institutionDone ? "Kurum hazır • komisyonları tamamlayın." : commissionDone ? "Komisyonlar hazır • kurum bilgisini tamamlayın." : "Kurum bilgisi ve iki komisyonu tamamlayın.");
        var productsValid = products.Count > 0 && products.All(p => DogruTemin.Core.Services.ProductUnits.IsSupported(p.Unit) && NumberParser.TryPositiveDecimal(p.Quantity, out _));
        var generalDone = !string.IsNullOrWhiteSpace(data.JobTitle) && productsValid && data.Dates.AssignmentDate.HasValue && !string.IsNullOrWhiteSpace(data.Dates.AssignmentNo) && data.Dates.ApprovalDate.HasValue && !string.IsNullOrWhiteSpace(data.Dates.ApprovalNo);
        SetHomeStage(View<System.Windows.Controls.TextBlock>("HomeStageGeneralStatus"), View<System.Windows.Controls.TextBlock>("HomeStageGeneralDetail"), generalDone, generalDone ? $"{products.Count} ürün ve temel işlem bilgileri hazır." : "İş başlığı, ürünler ve görevlendirme/harcama bilgilerini tamamlayın.");
        var firmsDone = firms.Count >= 3;
        var offersDone = firmsDone && products.Count > 0 && firms.All(f => products.All(p =>
        {
            var offer = f.Offers.FirstOrDefault(o => o.ProductId == p.Id);
            return offer?.ApproximateUnitPrice > 0m && offer.FinalUnitPrice > 0m;
        }));
        SetHomeCombinedStage(View<System.Windows.Controls.TextBlock>("HomeStageFirmsStatus"), View<System.Windows.Controls.TextBlock>("HomeStageFirmsDetail"), firmsDone, offersDone, $"{firms.Count} firma ve tüm teklifler hazır.", firmsDone ? $"{firms.Count} firma hazır • tekliflerde eksik var." : $"En az 3 firma gerekli; şu an {firms.Count} firma var.");
        var dateWarnings = ValidationService.DateLogicWarnings(data);
        var datesDone = data.Dates.AssignmentDate.HasValue && data.Dates.CostCalculationDate.HasValue && data.Dates.ApprovalDocumentDate.HasValue && data.Dates.MarketReportDate.HasValue && data.Dates.InspectionReportDate.HasValue && data.Dates.DeliveryDate.HasValue && dateWarnings.Count == 0;
        SetHomeStage(View<System.Windows.Controls.TextBlock>("HomeStageDatesStatus"), View<System.Windows.Controls.TextBlock>("HomeStageDatesDetail"), datesDone, datesDone ? "Belge tarihleri hazır ve zorunlu kronoloji geçerli." : "Kurul/komisyon, yaklaşık maliyet, Onay Belgesi, piyasa ve muayene/kabul tarihlerini kontrol edin.");
        var preparationTypes = new[]
        {
            OutputDocumentType.ApproximateRequest,
            OutputDocumentType.FirmOfferForm
        };
        var finalProcessTypes = new[]
        {
            OutputDocumentType.ApproximateCostSheet,
            OutputDocumentType.ApprovalDocument,
            OutputDocumentType.MarketResearchReport,
            OutputDocumentType.InspectionAcceptanceReport
        };
        var preparationIssues = DocumentReadinessService.GetExportCoreIssues(data, preparationTypes);
        var finalProcessIssues = DocumentReadinessService.GetExportCoreIssues(data, finalProcessTypes);
        var preparationReady = preparationIssues.Count == 0;
        var finalProcessReady = finalProcessIssues.Count == 0;
        var outputDone = preparationReady && finalProcessReady;
        SetHomeOutputStage(preparationReady, finalProcessReady, preparationIssues.Count, finalProcessIssues.Count);
        var firmsOffersDone = firmsDone && offersDone;
        var doneCount = new[]
        {
            institutionCommissionDone,
            generalDone,
            firmsOffersDone,
            datesDone,
            outputDone
        }.Count(x => x);
        View<System.Windows.Controls.TextBlock>("HomeStageSummaryText").Text = $"{doneCount} / 5 TAMAM";
        if (!institutionCommissionDone)
        {
            View<System.Windows.Controls.TextBlock>("HomeNextStepText").Text = "Kurum bilgisi ile piyasa ve muayene/kabul komisyonlarını tamamlayın.";
        }
        else if (!generalDone)
        {
            View<System.Windows.Controls.TextBlock>("HomeNextStepText").Text = "İşlem, ürün ve görevlendirme/harcama bilgilerini tamamlayın.";
        }
        else if (!firmsDone)
        {
            View<System.Windows.Controls.TextBlock>("HomeNextStepText").Text = "En az 3 firma ekleyin.";
        }
        else if (!offersDone)
        {
            View<System.Windows.Controls.TextBlock>("HomeNextStepText").Text = "Firmaların yaklaşık ve nihai tekliflerini tamamlayın.";
        }
        else if (!data.Dates.ApprovalDocumentDate.HasValue || string.IsNullOrWhiteSpace(data.ApprovalDocument.AvailableBudgetAmount) || string.IsNullOrWhiteSpace(data.ApprovalDocument.SubmittedByName) || string.IsNullOrWhiteSpace(data.ApprovalDocument.SubmittedByTitle))
        {
            View<System.Windows.Controls.TextBlock>("HomeNextStepText").Text = "Onay Belgesi'ne özgü bilgileri tamamlayın.";
        }
        else if (!datesDone)
        {
            View<System.Windows.Controls.TextBlock>("HomeNextStepText").Text = "Belge tarihlerini girin ve tarih sırasını kontrol edin.";
        }
        else
        {
            View<System.Windows.Controls.TextBlock>("HomeNextStepText").Text = outputDone ? "Süreç hazır. Resmî belgeleri önizleyebilir, PDF kaydedebilir veya yazdırabilirsiniz." : "Çıktı ekranındaki kritik eksikleri giderin ve belgeleri önizleyin.";
        }
    }

    internal static void SetHomeCombinedStage(TextBlock statusBlock, TextBlock detailBlock, bool firstReady, bool secondReady, string readyDetail, string incompleteDetail)
    {
        if (firstReady && secondReady)
        {
            statusBlock.Text = "TAMAM";
            statusBlock.Foreground = new SolidColorBrush(Color.FromRgb(126, 226, 166));
            detailBlock.Text = readyDetail;
            return;
        }

        if (firstReady || secondReady)
        {
            statusBlock.Text = "KISMİ";
            statusBlock.Foreground = new SolidColorBrush(Color.FromRgb(143, 183, 216));
        }
        else
        {
            statusBlock.Text = "EKSİK";
            statusBlock.Foreground = new SolidColorBrush(Color.FromRgb(255, 191, 54));
        }

        detailBlock.Text = incompleteDetail;
    }

    internal static void SetHomeStage(TextBlock statusBlock, TextBlock detailBlock, bool completed, string detail)
    {
        statusBlock.Text = completed ? "TAMAM" : "EKSİK";
        statusBlock.Foreground = completed ? new SolidColorBrush(Color.FromRgb(126, 226, 166)) : new SolidColorBrush(Color.FromRgb(255, 191, 54));
        detailBlock.Text = detail;
    }

    internal void SetHomeOutputStage(bool preparationReady, bool finalReady, int preparationIssueCount, int finalIssueCount)
    {
        if (preparationReady && finalReady)
        {
            View<System.Windows.Controls.TextBlock>("HomeStageOutputStatus").Text = "TAMAM";
            View<System.Windows.Controls.TextBlock>("HomeStageOutputStatus").Foreground = new SolidColorBrush(Color.FromRgb(126, 226, 166));
            View<System.Windows.Controls.TextBlock>("HomeStageOutputDetail").Text = "Ön hazırlık ve nihai işlem belgeleri üretime hazır.";
            return;
        }

        if (preparationReady || finalReady)
        {
            View<System.Windows.Controls.TextBlock>("HomeStageOutputStatus").Text = "KISMİ";
            View<System.Windows.Controls.TextBlock>("HomeStageOutputStatus").Foreground = new SolidColorBrush(Color.FromRgb(143, 183, 216));
        }
        else
        {
            View<System.Windows.Controls.TextBlock>("HomeStageOutputStatus").Text = "EKSİK";
            View<System.Windows.Controls.TextBlock>("HomeStageOutputStatus").Foreground = new SolidColorBrush(Color.FromRgb(255, 191, 54));
        }

        var preparationText = preparationReady ? "Ön hazırlık: hazır" : $"Ön hazırlık: {preparationIssueCount} eksik";
        var finalText = finalReady ? "Nihai: hazır" : $"Nihai: {finalIssueCount} eksik";
        View<System.Windows.Controls.TextBlock>("HomeStageOutputDetail").Text = $"{preparationText} • {finalText}";
    }

    internal void UpdateHomeActivitySummary()
    {
        if (View<System.Windows.Controls.TextBlock>("HomeHeroLastSavedText")is null)
            return;
        View<System.Windows.Controls.TextBlock>("HomeHeroLastSavedText").Text = Context.Session.LastSavedAt.HasValue ? Context.Session.LastSavedAt.Value.ToString("dd.MM.yyyy • HH:mm", CultureInfo.InvariantCulture) : "Henüz kaydedilmedi";
    }

    internal void UpdateDerivedState(ProcurementData? supplied = null)
    {
        if (Context.State.SuppressDirtyTracking && supplied is null)
            return;
        var data = supplied ?? BuildDataFromUi();
        var calc = ProcurementEngine.Calculate(data);
        var readiness = ValidationService.ReadinessPercent(data);
        var filledCommission = data.InspectionCommission.Count(m => !string.IsNullOrWhiteSpace(m.Name) && !string.IsNullOrWhiteSpace(m.Title)) + data.MarketCommission.Count(m => !string.IsNullOrWhiteSpace(m.Name) && !string.IsNullOrWhiteSpace(m.Title));
        var firmCount = data.Firms.Count(f => !string.IsNullOrWhiteSpace(f.Name));
        var productCount = data.Products.Count(p => !string.IsNullOrWhiteSpace(p.Name));
        View<System.Windows.Controls.TextBlock>("HomeProductCountText").Text = productCount.ToString(CultureInfo.InvariantCulture);
        View<System.Windows.Controls.TextBlock>("HomeProductCountHintText").Text = productCount > 0 ? $"{productCount} / 10 kayıtlı" : "En az 1 gerekli";
        View<System.Windows.Controls.TextBlock>("HomeFirmCountText").Text = firmCount.ToString(CultureInfo.InvariantCulture);
        View<System.Windows.Controls.TextBlock>("HomeFirmCountHintText").Text = firmCount >= 3 ? $"{firmCount} firma hazır" : $"{firmCount} / en az 3";
        View<System.Windows.Controls.TextBlock>("ActiveOperationText").Text = !string.IsNullOrWhiteSpace(data.JobTitle) ? data.JobTitle : (!string.IsNullOrWhiteSpace(data.OperationNumber) ? data.OperationNumber : "Yeni İşlem");
        View<System.Windows.Controls.TextBlock>("HeaderReadinessText").Text = $"%{readiness}";
        UpdateReadinessRing(readiness);
        View<System.Windows.Controls.TextBlock>("HomeJobTitleText").Text = string.IsNullOrWhiteSpace(data.JobTitle) ? "Henüz aktif işlem başlığı girilmedi" : data.JobTitle;
        View<System.Windows.Controls.TextBlock>("HomeApprovalDateText").Text = data.Dates.ApprovalDate.HasValue ? $"Harcama belgesi tarihi: {data.Dates.ApprovalDate:dd.MM.yyyy}" : "Harcama belgesi tarihi: Henüz belirtilmedi";
        View<System.Windows.Controls.TextBlock>("HomeOperationNumberText").Text = string.IsNullOrWhiteSpace(data.OperationNumber) ? "Henüz yok" : data.OperationNumber;
        View<System.Windows.Controls.TextBlock>("HomeApproximateTotalText").Text = calc.ApproximateCostTotal > 0m ? NumberParser.Money(calc.ApproximateCostTotal) : "—";
        View<System.Windows.Controls.TextBlock>("HomeFinalTotalText").Text = calc.MarketResultTotal > 0m ? NumberParser.Money(calc.MarketResultTotal) : "—";
        if (calc.ApproximateCostTotal > 0m && calc.MarketResultTotal > 0m)
        {
            var difference = calc.ApproximateCostTotal - calc.MarketResultTotal;
            var differencePercent = calc.ApproximateCostTotal == 0m ? 0m : difference / calc.ApproximateCostTotal * 100m;
            View<System.Windows.Controls.TextBlock>("HomeDifferenceText").Text = NumberParser.Money(difference);
            View<System.Windows.Controls.TextBlock>("HomeDifferenceText").Foreground = difference >= 0m ? new SolidColorBrush(Color.FromRgb(126, 226, 166)) : new SolidColorBrush(Color.FromRgb(255, 165, 165));
            View<System.Windows.Controls.TextBlock>("HomeDifferenceHintText").Text = difference >= 0m ? $"Yaklaşık maliyete göre %{Math.Abs(differencePercent):0.0} düşük" : $"Yaklaşık maliyete göre %{Math.Abs(differencePercent):0.0} yüksek";
        }
        else
        {
            View<System.Windows.Controls.TextBlock>("HomeDifferenceText").Text = "—";
            View<System.Windows.Controls.TextBlock>("HomeDifferenceText").Foreground = new SolidColorBrush(Color.FromRgb(255, 226, 161));
            View<System.Windows.Controls.TextBlock>("HomeDifferenceHintText").Text = "Hesaplama bekleniyor";
        }

        if (calc.HasWinnerTie && string.IsNullOrWhiteSpace(calc.WinningFirmId))
        {
            View<System.Windows.Controls.TextBlock>("HomeWinnerText").Text = "Seçim gerekli";
            View<System.Windows.Controls.TextBlock>("HomeWinnerHintText").Text = "Eşit en düşük teklif var";
        }
        else if (!string.IsNullOrWhiteSpace(calc.WinningFirmName))
        {
            View<System.Windows.Controls.TextBlock>("HomeWinnerText").Text = calc.WinningFirmName;
            View<System.Windows.Controls.TextBlock>("HomeWinnerHintText").Text = calc.MarketResultTotal > 0m ? NumberParser.Money(calc.MarketResultTotal) : "Nihai toplam hazır";
        }
        else
        {
            View<System.Windows.Controls.TextBlock>("HomeWinnerText").Text = "Henüz yok";
            View<System.Windows.Controls.TextBlock>("HomeWinnerHintText").Text = firmCount < 3 ? "En az 3 firma gerekli" : "Nihai teklifler bekleniyor";
        }

        var workflowIssues = ValidationService.ValidateWorkflow(data);
        var homeWarningCount = workflowIssues.Count;
        View<System.Windows.Controls.TextBlock>("HomeWarningCountText").Text = $"{homeWarningCount} UYARI";
        UpdateHomeActivitySummary();
        View<System.Windows.Controls.TextBlock>("HomeReadinessText").Text = $"%{readiness}";
        View<System.Windows.Controls.Border>("HomeReadinessBar").Width = 690.0 * readiness / 100.0;
        View<System.Windows.Controls.TextBlock>("HomeReadinessHintText").Text = readiness >= 100 && homeWarningCount == 0 ? "Temel hazırlık tamam. Akıllı iş akışı tüm aşamaların durumunu tek bakışta gösteriyor." : homeWarningCount > 0 ? $"{homeWarningCount} kontrol noktası dikkat istiyor. Akıllı iş akışı ilk eksik aşamayı gösteriyor." : "Süreç bilgileri girildikçe hazırlık durumu otomatik güncellenir.";
        UpdateHomeProcessMap(data);
        Context.Documents.InvalidateDocumentPreviews();
    }

    internal void UpdateReadinessRing(int percent)
    {
        var p = Math.Clamp(percent, 0, 100);
        if (p <= 0)
        {
            View<System.Windows.Shapes.Path>("HeaderReadinessArc").Data = Geometry.Empty;
            return;
        }

        var angle = p >= 100 ? 359.99 : 360.0 * p / 100.0;
        var radians = (angle - 90.0) * Math.PI / 180.0;
        var x = 24.0 + 20.0 * Math.Cos(radians);
        var y = 24.0 + 20.0 * Math.Sin(radians);
        var large = angle > 180.0 ? 1 : 0;
        View<System.Windows.Shapes.Path>("HeaderReadinessArc").Data = Geometry.Parse(string.Format(CultureInfo.InvariantCulture, "M24,4 A20,20 0 {0} 1 {1:0.###},{2:0.###}", large, x, y));
    }
}
