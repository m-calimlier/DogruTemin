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

namespace DogruTemin.Wpf.Features.Catalog;
internal sealed partial class CatalogModule
{
    internal bool HasFirmEditorContent() => !string.IsNullOrWhiteSpace(View<System.Windows.Controls.TextBox>("FirmNameBox")?.Text) || !string.IsNullOrWhiteSpace(View<System.Windows.Controls.TextBox>("ContactNameBox")?.Text) || !string.IsNullOrWhiteSpace(View<System.Windows.Controls.TextBox>("TaxNoBox")?.Text) || !string.IsNullOrWhiteSpace(View<System.Windows.Controls.TextBox>("PhoneBox")?.Text) || !string.IsNullOrWhiteSpace(View<System.Windows.Controls.TextBox>("AddressBox")?.Text) || !string.IsNullOrWhiteSpace(View<System.Windows.Controls.TextBox>("EmailBox")?.Text);
    internal bool HasProductEditorContent()
    {
        var quantity = View<System.Windows.Controls.TextBox>("ProductQuantityBox")?.Text?.Trim() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(View<System.Windows.Controls.TextBox>("ProductNameBox")?.Text) || !string.IsNullOrWhiteSpace(View<System.Windows.Controls.TextBox>("ProductSpecBox")?.Text) || (quantity.Length > 0 && quantity != "1") || (View<System.Windows.Controls.ComboBox>("ProductUnitBox")?.SelectedIndex ?? 0) != 0;
    }
}
