using DogruTemin.Wpf.Models;
using ProcurementData = DogruTemin.Core.Models.ProcurementData;
using ValidationSeverity = DogruTemin.Core.Models.ValidationSeverity;
using OutputDocumentType = DogruTemin.Core.Models.OutputDocumentType;
using FirmOfferCore = DogruTemin.Core.Models.FirmOffer;
using AppSettings = DogruTemin.Core.Models.ApplicationSettings;
using DogruTemin.Core.Services;
using DogruTemin.Wpf.Services;
using Microsoft.Win32;
using System.Windows.Documents;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Interop;

namespace DogruTemin.Wpf.Features.Workspace;
internal sealed partial class WorkspaceModule
{
    internal void InitializeBlankState(AppSettings? settings = null)
    {
        Context.State.Firms.Clear();
        Context.State.Products.Clear();
        Context.State.InspectionCommission.Clear();
        Context.State.MarketCommission.Clear();
        Context.State.OfferRows.Clear();
        Context.Catalog._editingFirm = null;
        Context.Catalog._editingProduct = null;
        Context.Offers._offerEditingFirm = null;
        Context.Session.OfferEditorDirty = false;
        Context.Session.PreferredWinnerFirmId = string.Empty;
        Context.Session.RecordId = string.Empty;
        Context.Session.CreatedAt = DateTime.Now;
        settings ??= Context.State.Settings;
        for (var i = 1; i <= 3; i++)
        {
            Context.State.InspectionCommission.Add(new CommissionMember { No = i });
            Context.State.MarketCommission.Add(new CommissionMember { No = i });
        }

        View<DogruTemin.Wpf.LargeDatePicker>("AssignmentDatePicker").SelectedDate = null;
        View<DogruTemin.Wpf.LargeDatePicker>("ApprovalDatePicker").SelectedDate = null;
        View<DogruTemin.Wpf.LargeDatePicker>("CostCalculationDatePicker").SelectedDate = null;
        View<DogruTemin.Wpf.LargeDatePicker>("ApprovalDocumentDatePicker").SelectedDate = null;
        View<DogruTemin.Wpf.LargeDatePicker>("MarketReportDatePicker").SelectedDate = null;
        View<DogruTemin.Wpf.LargeDatePicker>("InspectionReportDatePicker").SelectedDate = null;
        View<DogruTemin.Wpf.LargeDatePicker>("DeliveryDatePicker").SelectedDate = null;
        View<System.Windows.Controls.TextBox>("JobTitleBox").Text = string.Empty;
        View<System.Windows.Controls.TextBox>("AssignmentNoBox").Text = string.Empty;
        View<System.Windows.Controls.TextBox>("ApprovalNoBox").Text = string.Empty;
        View<System.Windows.Controls.TextBox>("ApprovalDocumentNumberBox").Text = string.Empty;
        View<System.Windows.Controls.TextBox>("ApprovalAvailableBudgetBox").Text = string.Empty;
        View<System.Windows.Controls.TextBox>("ApprovalInvestmentProjectBox").Text = string.Empty;
        View<System.Windows.Controls.TextBox>("ApprovalBudgetCodeBox").Text = string.Empty;
        View<System.Windows.Controls.TextBox>("ApprovalAdvanceConditionsBox").Text = string.Empty;
        View<System.Windows.Controls.TextBox>("ApprovalTenderProcedureBox").Text = "4734 S.K. 22/d Kapsamında";
        View<System.Windows.Controls.TextBox>("ApprovalAnnouncementMethodBox").Text = "İlansız";
        View<System.Windows.Controls.TextBox>("ApprovalPrequalificationSalePriceBox").Text = string.Empty;
        View<System.Windows.Controls.TextBox>("ApprovalPriceDifferenceBasisBox").Text = "Ödenmeyecektir";
        View<System.Windows.Controls.TextBox>("ApprovalNotesBox").Text = string.Empty;
        View<System.Windows.Controls.TextBox>("ApprovalSubmittedByNameBox").Text = settings.SubmittedByName;
        View<System.Windows.Controls.TextBox>("ApprovalSubmittedByTitleBox").Text = settings.SubmittedByTitle;
        var preservedInstitution = settings.Institution;
        View<System.Windows.Controls.TextBox>("InstitutionTopAuthorityBox").Text = preservedInstitution?.TopAuthority ?? string.Empty;
        View<System.Windows.Controls.TextBox>("InstitutionFullNameBox").Text = !string.IsNullOrWhiteSpace(preservedInstitution?.FullName) ? preservedInstitution!.FullName : preservedInstitution?.ShortName ?? string.Empty;
        View<System.Windows.Controls.TextBox>("InstitutionProvinceBox").Text = preservedInstitution?.Province ?? string.Empty;
        View<System.Windows.Controls.TextBox>("InstitutionAddressBox").Text = preservedInstitution?.Address ?? string.Empty;
        View<System.Windows.Controls.TextBox>("InstitutionPhoneBox").Text = preservedInstitution?.Phone ?? string.Empty;
        View<System.Windows.Controls.TextBox>("InstitutionEmailBox").Text = preservedInstitution?.Email ?? string.Empty;
        View<System.Windows.Controls.TextBox>("AuthorizedNameBox").Text = preservedInstitution?.AuthorizedName ?? string.Empty;
        View<System.Windows.Controls.TextBox>("AuthorizedDutyBox").Text = preservedInstitution?.AuthorizedDuty ?? string.Empty;
        View<System.Windows.Controls.TextBox>("AuthorizedTitleBox").Text = preservedInstitution?.AuthorizedTitle ?? string.Empty;
        View<System.Windows.Controls.CheckBox>("AutoBackupCheckBox").IsChecked = true;
        Context.Session.OperationNumber = string.Empty;
        Context.Catalog.ClearFirmForm();
        Context.Catalog.ClearProductForm();
    }
}
