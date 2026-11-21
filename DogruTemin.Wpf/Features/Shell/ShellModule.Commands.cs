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

namespace DogruTemin.Wpf.Features.Shell;
internal sealed partial class ShellModule
{
    internal void ToggleMaximizeRestore()
    {
        Context.Window.WindowState = Context.Window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    internal void Window_StateChanged(object sender, EventArgs e)
    {
        UpdateMaximizeRestoreButton();
        Context.Window.Dispatcher.BeginInvoke(new Action(() =>
        {
            View<System.Windows.Controls.Grid>("AppScaleViewbox").InvalidateMeasure();
            View<System.Windows.Controls.Grid>("AppScaleViewbox").InvalidateArrange();
        }));
    }

    internal void UpdateMaximizeRestoreButton()
    {
        if (View<System.Windows.Controls.TextBlock>("MaximizeRestoreIcon")is null || View<System.Windows.Controls.Button>("MaximizeRestoreButton")is null)
            return;
        var isMaximized = Context.Window.WindowState == WindowState.Maximized;
        View<System.Windows.Controls.TextBlock>("MaximizeRestoreIcon").Text = isMaximized ? "\uE923" : "\uE922";
        View<System.Windows.Controls.Button>("MaximizeRestoreButton").ToolTip = isMaximized ? "Geri yükle" : "Tam ekran";
    }

    internal void Close_Click(object sender, RoutedEventArgs e)
    {
        if (!Context.Workspace.ConfirmUnsavedWork("kapat", "DoğruTemin - Kapat"))
            return;
        Context.State.CloseApproved = true;
        RecoverySessionService.EndCleanly();
        Context.Window.Close();
    }

    internal void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (Context.State.CloseApproved)
            return;
        if (!Context.Workspace.ConfirmUnsavedWork("kapat", "DoğruTemin - Kapat"))
        {
            e.Cancel = true;
            return;
        }

        Context.State.CloseApproved = true;
        RecoverySessionService.EndCleanly();
    }

    internal void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.N)
        {
            Context.Workspace.NewOperation_Click(Context.Window, new RoutedEventArgs());
            e.Handled = true;
            return;
        }

        if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
        {
            e.Handled = true;
            Context.Workspace.Save_Click(Context.Window, new RoutedEventArgs());
            return;
        }

        if (e.Key != Key.Enter || Keyboard.Modifiers != ModifierKeys.None)
            return;
        if (_currentView is not ("Institution" or "General" or "Firms" or "Offers" or "ApprovalDocument" or "Dates" or "Output"))
            return;
        if (e.OriginalSource is not TextBox textBox || textBox.AcceptsReturn)
            return;
        if (FindVisualParent<DataGrid>(textBox)is not null)
            return;
        textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        e.Handled = true;
    }

    internal static T? FindVisualParent<T>(DependencyObject child)
        where T : DependencyObject
    {
        DependencyObject? current = child;
        while (current is not null)
        {
            if (current is T match)
                return match;
            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }
}
