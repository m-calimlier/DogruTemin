using DogruTemin.Wpf.Models;
using System.Windows.Interop;
using System.ComponentModel;

namespace DogruTemin.Wpf.Features.Catalog;
internal sealed partial class CatalogModule : global::DogruTemin.Wpf.Features.FeatureModule
{
    internal CatalogModule(WorkspaceContext context) : base(context)
    {
    }

    internal Firm? _editingFirm;
    internal ProductItem? _editingProduct;
}
