using DogruTemin.Wpf.Models;
using System.Windows.Interop;
using System.ComponentModel;

namespace DogruTemin.Wpf.Features.Offers;
internal sealed partial class OffersModule : global::DogruTemin.Wpf.Features.FeatureModule
{
    internal OffersModule(WorkspaceContext context) : base(context)
    {
    }

    internal Firm? _offerEditingFirm;
    internal bool _suppressOfferEditorEvents;
    internal bool _suppressOfferSelection;
}
