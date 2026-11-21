using DogruTemin.Core.Modules.Archive;
using DogruTemin.Wpf.Models;
using DogruTemin.Core.Services;
using DogruTemin.Wpf.Services;
using Microsoft.Win32;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DogruTemin.Wpf.Features.Workspace;
internal sealed partial class WorkspaceModule
{
    internal bool ConfirmRecordSwitch() => ConfirmUnsavedWork("devam et", "DoğruTemin - İşlem değiştir");
    internal bool ConfirmUnsavedWork(string actionText, string title)
    {
        if (!HasUnsavedWork())
            return true;
        var result = MessageBox.Show($"Kaydedilmemiş değişiklikler var.\n\nEVET: Kaydet ve {actionText}\nHAYIR: Kaydetmeden {actionText}\nİPTAL: Programa dön", title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
        if (result == MessageBoxResult.Cancel)
            return false;
        if (result == MessageBoxResult.Yes)
        {
            Save_Click(Context.Window, new RoutedEventArgs());
            return !HasUnsavedWork();
        }

        return true;
    }

    internal bool HasUnsavedWork() => Context.Session.HasUnsavedWork;
    internal void NewOperation_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmRecordSwitch())
            return;
        Context.Shell.ReportStartup(64, "Ayarlar yükleniyor...");
        Context.State.Settings = Context.Storage.LoadSettings();
        Context.State.SuppressDirtyTracking = true;
        InitializeBlankState(Context.State.Settings);
        Context.State.SuppressDirtyTracking = false;
        Context.Storage.ClearActiveFile();
        Context.Session.ArchiveEditRecordId = null;
        Context.Session.Revision = 0;
        Context.Session.LastOutput = null;
        UpdateDerivedState();
        Context.Session.RecordDirty = true;
        ShowDirtyStatus("Kaydedilmedi • Yeni işlem");
        Context.Shell.ShowView("General");
    }
}
