using DogruTemin.Wpf.Models;
using ProcurementData = DogruTemin.Core.Models.ProcurementData;
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
    internal void Save_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (Context.Session.FirmEditorDirty)
            {
                MessageBox.Show("Firma kartında henüz listeye aktarılmamış bilgi var. Önce firma bölümündeki EKLE/KAYDET düğmesiyle firma girişini tamamlayın.", "DoğruTemin", MessageBoxButton.OK, MessageBoxImage.Information);
                Context.Shell.ShowView("Firms");
                return;
            }

            if (Context.Session.ProductEditorDirty)
            {
                MessageBox.Show("Ürün kartında henüz listeye aktarılmamış bilgi var. Önce ürün bölümündeki EKLE/KAYDET düğmesiyle ürün girişini tamamlayın.", "DoğruTemin", MessageBoxButton.OK, MessageBoxImage.Information);
                Context.Shell.ShowView("General");
                return;
            }

            if (Context.Session.OfferEditorDirty)
            {
                View<System.Windows.Controls.DataGrid>("OfferGrid").CommitEdit(DataGridEditingUnit.Cell, true);
                View<System.Windows.Controls.DataGrid>("OfferGrid").CommitEdit(DataGridEditingUnit.Row, true);
                if (!Context.Offers.SaveOfferEditor(true))
                    return;
            }

            var data = BuildDataFromUi();
            var datesConfirmed = false;
            var saveResult = Context.Records.Save(data, data.AutoBackup);
            if (saveResult.RequiresWarningConfirmation)
            {
                var answer = MessageBox.Show("Tarih sıralamasında kontrol edilmesi gereken noktalar var:\n\n" + string.Join("\n", saveResult.Warnings.Select(i => "• " + i.Message)) + "\n\nYine de taslak kaydedilsin mi?", "DoğruTemin - Tarih sırası", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (answer != MessageBoxResult.Yes)
                    return;
                datesConfirmed = true;
                saveResult = Context.Records.Save(data, data.AutoBackup, allowDateWarnings: true);
            }

            if (saveResult.RequiresReplacementConfirmation && Context.Session.ArchiveEditRecordId == data.RecordId)
                saveResult = Context.Records.Save(data, data.AutoBackup, allowDateWarnings: datesConfirmed, confirmReplacement: true);
            if (saveResult.RequiresReplacementConfirmation)
            {
                var answer = MessageBox.Show($"'{data.JobTitle}' işleminin kayıtlı bilgileri değiştirilecek.\n\nDüzeltilmiş kayıt mevcut kaydın yerini alacak; arşivde ayrı bir eski sürüm tutulmayacak. Önceki PDF ve basılı belgeler yeni bilgileri temsil etmeyecek; belgeleri yeniden üretmeniz gerekir. Mevcut güvenlik yedekleme ayarı korunur.\n\nDeğişiklikler kaydedilsin mi?", "DoğruTemin - Kaydın üzerine yazma onayı", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                if (answer != MessageBoxResult.Yes)
                    return;
                saveResult = Context.Records.Save(data, data.AutoBackup, allowDateWarnings: datesConfirmed, confirmReplacement: true);
            }

            if (!saveResult.SavedSuccessfully)
            {
                MessageBox.Show(string.Join("\n", saveResult.BlockingIssues.Take(ProcurementLimits.MaxProducts).Select(i => "• " + i.Message)), "DoğruTemin - Geçersiz veri", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var savedData = saveResult.Data!;
            Context.Session.RecordId = savedData.RecordId;
            Context.Session.Revision = savedData.Revision;
            Context.Session.LastOutput = savedData.LastOutput;
            Context.Session.CreatedAt = savedData.CreatedAt;
            UpdateDerivedState(savedData);
            SetSavedStatus(savedData.LastSavedAt);
            Context.Archive.RefreshArchiveRecords();
            if (Context.Session.ArchiveEditRecordId == savedData.RecordId)
                UiNotificationService.Show(Context.Window, "Değişiklikler kaydedildi. İşlem arşivi güncellendi.", TimeSpan.FromSeconds(5));
            if (!string.IsNullOrWhiteSpace(saveResult.BackupWarning))
            {
                MessageBox.Show("Kayıt tamamlandı; ancak otomatik yedekleme sırasında uyarı oluştu:\n\n" + saveResult.BackupWarning, "DoğruTemin - Yedekleme Uyarısı", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            SetSaveErrorStatus();
            MessageBox.Show("Kayıt sırasında hata oluştu: " + ex.Message, "DoğruTemin", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
