using DogruTemin.Wpf.Models;
using System.Windows.Interop;
using System.ComponentModel;

namespace DogruTemin.Wpf.Features.Shell;
internal sealed partial class ShellModule : global::DogruTemin.Wpf.Features.FeatureModule
{
    internal ShellModule(WorkspaceContext context) : base(context)
    {
    }

    internal string _currentView = "Home";
    internal HwndSource? _windowSource;
    private const int WmGetMinMaxInfo = 0x0024;
    private const uint MonitorDefaultToNearest = 0x00000002;
}
