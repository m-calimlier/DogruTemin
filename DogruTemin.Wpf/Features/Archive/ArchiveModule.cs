using DogruTemin.Wpf.Models;
using System.Windows.Interop;
using System.ComponentModel;

namespace DogruTemin.Wpf.Features.Archive;
internal sealed partial class ArchiveModule : global::DogruTemin.Wpf.Features.FeatureModule
{
    internal ArchiveModule(WorkspaceContext context) : base(context)
    {
    }

    internal ICollectionView? _archiveView;
}
