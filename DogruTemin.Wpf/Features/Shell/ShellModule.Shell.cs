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

namespace DogruTemin.Wpf.Features.Shell;
internal sealed partial class ShellModule
{
    internal void ReportStartup(int percent, string stage)
    {
        try
        {
            Context.StartupProgress?.Invoke(percent, stage);
        }
        catch
        {
        }
    }

    internal void ApplyTaskbarIcon()
    {
        try
        {
            var iconUri = new Uri("pack://application:,,,/Assets/AppIcon.ico", UriKind.Absolute);
            Context.Window.Icon = BitmapFrame.Create(iconUri);
        }
        catch
        {
        // The XAML/ApplicationIcon fallback remains active if the runtime icon cannot be loaded.
        }
    }

    internal void ConfigureFixedWindowForCurrentScreen()
    {
        ResponsiveLayoutService.ApplyPreference(Context.Window, "Auto");
    }

    internal void MainWindow_SourceInitialized(object? sender, EventArgs e)
    {
        _windowSource = HwndSource.FromHwnd(new WindowInteropHelper(Context.Window).Handle);
        _windowSource?.AddHook(WindowMessageHook);
    }

    internal IntPtr WindowMessageHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg != WmGetMinMaxInfo || lParam == IntPtr.Zero)
            return IntPtr.Zero;
        var minMax = Marshal.PtrToStructure<MinMaxInfo>(lParam);
        var monitor = MonitorFromWindow(hwnd, MonitorDefaultToNearest);
        if (monitor != IntPtr.Zero)
        {
            var monitorInfo = new MonitorInfo
            {
                cbSize = Marshal.SizeOf<MonitorInfo>()
            };
            if (GetMonitorInfo(monitor, ref monitorInfo))
            {
                var work = monitorInfo.rcWork;
                var bounds = monitorInfo.rcMonitor;
                minMax.ptMaxPosition.x = work.left - bounds.left;
                minMax.ptMaxPosition.y = work.top - bounds.top;
                minMax.ptMaxSize.x = work.right - work.left;
                minMax.ptMaxSize.y = work.bottom - work.top;
            }
        }

        Marshal.StructureToPtr(minMax, lParam, false);
        handled = true;
        return IntPtr.Zero;
    }

    [DllImport("user32.dll")]
    internal static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpmi);
    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MinMaxInfo
    {
        public NativePoint ptReserved;
        public NativePoint ptMaxSize;
        public NativePoint ptMaxPosition;
        public NativePoint ptMinTrackSize;
        public NativePoint ptMaxTrackSize;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MonitorInfo
    {
        public int cbSize;
        public NativeRect rcMonitor;
        public NativeRect rcWork;
        public uint dwFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    internal void ShowView(string view)
    {
        if (view == "Commission")
            view = "Institution";
        _currentView = view;
        View<System.Windows.Controls.Grid>("WorkspaceLayout").MinWidth = view is "Settings" or "AboutSafety" ? 0 : 700;
        View<System.Windows.Controls.ScrollViewer>("HomeView").Visibility = view == "Home" ? Visibility.Visible : Visibility.Collapsed;
        View<System.Windows.Controls.ScrollViewer>("GeneralView").Visibility = view == "General" ? Visibility.Visible : Visibility.Collapsed;
        View<System.Windows.Controls.ScrollViewer>("FirmsOffersView").Visibility = view is "Firms" or "Offers" ? Visibility.Visible : Visibility.Collapsed;
        View<System.Windows.Controls.ScrollViewer>("DocumentsDatesView").Visibility = view is "Dates" or "Output" ? Visibility.Visible : Visibility.Collapsed;
        View<System.Windows.Controls.ScrollViewer>("ApprovalDocumentView").Visibility = view == "ApprovalDocument" ? Visibility.Visible : Visibility.Collapsed;
        View<System.Windows.Controls.ScrollViewer>("InstitutionView").Visibility = view == "Institution" ? Visibility.Visible : Visibility.Collapsed;
        View<System.Windows.Controls.ScrollViewer>("StatisticsView").Visibility = view == "Statistics" ? Visibility.Visible : Visibility.Collapsed;
        View<System.Windows.Controls.Grid>("ArchiveView").Visibility = view == "Archive" ? Visibility.Visible : Visibility.Collapsed;
        View<System.Windows.Controls.ScrollViewer>("SettingsView").Visibility = view == "Settings" ? Visibility.Visible : Visibility.Collapsed;
        View<System.Windows.Controls.Button>("NavSettings").Tag = view == "Settings" ? "Active" : null;
        View<System.Windows.Controls.ScrollViewer>("AboutSafetyView").Visibility = view == "AboutSafety" ? Visibility.Visible : Visibility.Collapsed;
        ResponsiveLayoutService.UpdateWorkspace(Context.Window);
        View<System.Windows.Controls.ScrollViewer>("WorkspaceScroll").ScrollToHorizontalOffset(0);
        View<System.Windows.Controls.ScrollViewer>("WorkspaceScroll").ScrollToVerticalOffset(0);
        if (view == "Firms")
            Context.Window.Dispatcher.BeginInvoke(new Action(() => View<System.Windows.Controls.Grid>("FirmsView")?.BringIntoView()));
        if (view == "Offers")
            Context.Window.Dispatcher.BeginInvoke(new Action(() => View<System.Windows.Controls.Grid>("OffersView")?.BringIntoView()));
        View<System.Windows.Controls.Button>("NavHome").Tag = view == "Home" ? "Active" : null;
        View<System.Windows.Controls.Button>("NavInstitution").Tag = view == "Institution" ? "Active" : null;
        View<System.Windows.Controls.Button>("NavGeneral").Tag = view == "General" ? "Active" : null;
        View<System.Windows.Controls.Button>("NavFirmsOffers").Tag = view is "Firms" or "Offers" ? "Active" : null;
        View<System.Windows.Controls.Button>("NavApprovalDocument").Tag = view == "ApprovalDocument" ? "Active" : null;
        View<System.Windows.Controls.Button>("NavDocumentsDates").Tag = view is "Dates" or "Output" ? "Active" : null;
        View<System.Windows.Controls.Button>("NavStatistics").Tag = view == "Statistics" ? "Active" : null;
        View<System.Windows.Controls.Button>("NavArchive").Tag = view == "Archive" ? "Active" : null;
        View<System.Windows.Controls.Button>("NavAboutSafety").Tag = view == "AboutSafety" ? "Active" : null;
        SynchronizeOutputSubpageWithCurrentView();
        Context.Workspace.RefreshArchiveEditState();
        Context.Documents.InvalidateDocumentPreviews();
    }

    internal void NavSettings_Click(object sender, RoutedEventArgs e) => ShowView("Settings");
    internal void NavHome_Click(object sender, RoutedEventArgs e) => ShowView("Home");
    internal void NavGeneral_Click(object sender, RoutedEventArgs e) => ShowView("General");
    internal void NavInstitution_Click(object sender, RoutedEventArgs e)
    {
        ShowView("Institution");
        Context.Window.Dispatcher.BeginInvoke(new Action(() => View<System.Windows.Controls.ScrollViewer>("InstitutionView").ScrollToTop()));
    }

    internal void NavFirmsOffers_Click(object sender, RoutedEventArgs e)
    {
        ShowView("Firms");
        if (Context.Offers._offerEditingFirm is null)
            Context.Offers.PrepareOffersView();
    }

    internal void NavApprovalDocument_Click(object sender, RoutedEventArgs e) => ShowView("ApprovalDocument");
    internal void NavDocumentsDates_Click(object sender, RoutedEventArgs e) => ShowView("Dates");
    internal void NavOutput_Click(object sender, RoutedEventArgs e) => ShowView("Output");
    internal void NavStatistics_Click(object sender, RoutedEventArgs e)
    {
        Context.Statistics.RefreshStatisticsYears();
        Context.Statistics.RefreshStatistics();
        ShowView("Statistics");
    }

    internal void NavArchive_Click(object sender, RoutedEventArgs e)
    {
        Context.Archive.RefreshArchiveRecords();
        ShowView("Archive");
    }

    internal void NavAboutSafety_Click(object sender, RoutedEventArgs e)
    {
        Context.Safety.UpdateSafetyPanel(Context.SafetyService.CheckIntegrity());
        ShowView("AboutSafety");
    }

    internal void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ToggleMaximizeRestore();
            e.Handled = true;
            return;
        }

        if (Context.Window.WindowState == WindowState.Normal && e.ButtonState == MouseButtonState.Pressed)
            Context.Window.DragMove();
    }

    internal void Minimize_Click(object sender, RoutedEventArgs e)
    {
        Context.Window.WindowState = WindowState.Minimized;
    }

    internal void MaximizeRestore_Click(object sender, RoutedEventArgs e)
    {
        ToggleMaximizeRestore();
    }
}
