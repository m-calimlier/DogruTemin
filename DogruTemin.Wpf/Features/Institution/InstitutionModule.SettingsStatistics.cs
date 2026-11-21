using AppSettings = DogruTemin.Core.Models.ApplicationSettings;
using DogruTemin.Core.Services;
using DogruTemin.Wpf.Models;
using DogruTemin.Wpf.Services;
using Microsoft.Win32;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DogruTemin.Wpf.Features.Institution;
internal sealed partial class InstitutionModule
{
    internal DogruTemin.Core.Models.InstitutionInfo BuildInstitutionFromUi() => new()
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
    };
    internal void SaveApplicationSettings_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Context.State.Settings = new AppSettings
            {
                WindowSizePreference = Context.State.Settings.WindowSizePreference,
                Institution = BuildInstitutionFromUi(),
                DefaultUnit = "Adet",
                SubmittedByName = View<System.Windows.Controls.TextBox>("ApprovalSubmittedByNameBox").Text.Trim(),
                SubmittedByTitle = View<System.Windows.Controls.TextBox>("ApprovalSubmittedByTitleBox").Text.Trim(),
                DefaultInspectionCommission = new List<DogruTemin.Core.Models.CommissionMember>(),
                DefaultMarketCommission = new List<DogruTemin.Core.Models.CommissionMember>()
            };
            Context.Storage.SaveSettings(Context.State.Settings);
            UiNotificationService.Show(Context.Window, "Kurum bilgileri kaydedildi. Yeni işlemlerde otomatik kullanılacak.", TimeSpan.FromSeconds(4));
        }
        catch (Exception ex)
        {
            MessageBox.Show("Kurum bilgileri kaydedilemedi: " + ex.Message, "DoğruTemin", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal void ApplyWindowSettings_Click(object sender, RoutedEventArgs e)
    {
        var preference = (View<System.Windows.Controls.ComboBox>("WindowSizeChoice").SelectedItem as ComboBoxItem)?.Tag as string ?? "Auto";
        try
        {
            Context.State.Settings.WindowSizePreference = preference;
            Context.Storage.SaveSettings(Context.State.Settings);
            ResponsiveLayoutService.ApplyPreference(Context.Window, preference);
            UiNotificationService.Show(Context.Window, "Pencere ayarı kaydedildi ve uygulandı.", TimeSpan.FromSeconds(4));
        }
        catch (Exception ex)
        {
            MessageBox.Show("Pencere ayarı kaydedilemedi: " + ex.Message, "DoğruTemin", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
