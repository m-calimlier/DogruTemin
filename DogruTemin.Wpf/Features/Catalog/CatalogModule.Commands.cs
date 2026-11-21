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

namespace DogruTemin.Wpf.Features.Catalog;
internal sealed partial class CatalogModule
{
    internal void TransientFirmEditor_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (Context.State.SuppressDirtyTracking)
            return;
        Context.Session.FirmEditorDirty = _editingFirm is not null || HasFirmEditorContent();
        Context.Workspace.RefreshSaveStatusVisual();
    }

    internal void TransientProductEditor_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (Context.State.SuppressDirtyTracking)
            return;
        Context.Session.ProductEditorDirty = _editingProduct is not null || HasProductEditorContent();
        Context.Workspace.RefreshSaveStatusVisual();
    }

    internal void TransientProductEditor_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Context.State.SuppressDirtyTracking)
            return;
        Context.Session.ProductEditorDirty = _editingProduct is not null || HasProductEditorContent();
        Context.Workspace.RefreshSaveStatusVisual();
    }
}
