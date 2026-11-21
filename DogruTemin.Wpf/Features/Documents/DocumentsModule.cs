using DogruTemin.Wpf.Models;
using System.Windows.Interop;
using System.ComponentModel;

namespace DogruTemin.Wpf.Features.Documents;
internal sealed partial class DocumentsModule : global::DogruTemin.Wpf.Features.FeatureModule
{
    internal DocumentsModule(WorkspaceContext context) : base(context)
    {
    }

    internal string _lastDateSequenceWarning = string.Empty;
}
