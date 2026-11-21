using DogruTemin.Wpf.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DogruTemin.Core.Services;

namespace DogruTemin.Wpf.Features.Catalog;
internal sealed partial class CatalogModule
{
    internal void AddOrUpdateFirm_Click(object sender, RoutedEventArgs e)
    {
        var name = View<System.Windows.Controls.TextBox>("FirmNameBox").Text.Trim();
        if (name.Length == 0)
        {
            MessageBox.Show("Firma adını giriniz.", "DoğruTemin", MessageBoxButton.OK, MessageBoxImage.Warning);
            View<System.Windows.Controls.TextBox>("FirmNameBox").Focus();
            return;
        }

        if (!IsValidPhoneEntry(View<System.Windows.Controls.TextBox>("PhoneBox").Text))
        {
            MessageBox.Show("Telefon numarası boş bırakılabilir; girilirse (0) dışında 10 rakam olmalıdır.", "DoğruTemin", MessageBoxButton.OK, MessageBoxImage.Warning);
            View<System.Windows.Controls.TextBox>("PhoneBox").Focus();
            return;
        }

        if (_editingFirm is null && Context.State.Firms.Count >= 5)
        {
            MessageBox.Show("En fazla 5 firma eklenebilir.", "DoğruTemin", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var taxNo = View<System.Windows.Controls.TextBox>("TaxNoBox").Text.Trim();
        var duplicate = Context.State.Firms.FirstOrDefault(f => f != _editingFirm && (string.Equals(f.Name.Trim(), name, StringComparison.OrdinalIgnoreCase) || (taxNo.Length > 0 && string.Equals(f.TaxNo.Trim(), taxNo, StringComparison.OrdinalIgnoreCase))));
        if (duplicate is not null)
        {
            MessageBox.Show("Aynı firma adı veya vergi numarasıyla kayıtlı başka bir firma bulunuyor.", "DoğruTemin", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_editingFirm is null)
        {
            Context.State.Firms.Add(new Firm { No = Context.State.Firms.Count + 1, Name = name, TaxNo = taxNo, ContactName = View<System.Windows.Controls.TextBox>("ContactNameBox").Text.Trim(), Phone = PhoneForStorage(View<System.Windows.Controls.TextBox>("PhoneBox").Text), Email = View<System.Windows.Controls.TextBox>("EmailBox").Text.Trim(), Address = View<System.Windows.Controls.TextBox>("AddressBox").Text.Trim() });
        }
        else
        {
            _editingFirm.Name = name;
            _editingFirm.TaxNo = taxNo;
            _editingFirm.ContactName = View<System.Windows.Controls.TextBox>("ContactNameBox").Text.Trim();
            _editingFirm.Phone = PhoneForStorage(View<System.Windows.Controls.TextBox>("PhoneBox").Text);
            _editingFirm.Email = View<System.Windows.Controls.TextBox>("EmailBox").Text.Trim();
            _editingFirm.Address = View<System.Windows.Controls.TextBox>("AddressBox").Text.Trim();
            View<System.Windows.Controls.DataGrid>("FirmGrid").Items.Refresh();
        }

        Context.Session.PreferredWinnerFirmId = string.Empty;
        ReNumberFirms();
        if ((Context.Shell._currentView is "Firms" or "Offers") && Context.Offers._offerEditingFirm is null)
            Context.Offers.PrepareOffersView();
        Context.Workspace.MarkDirty();
        ClearFirmForm();
    }

    internal void RowEdit_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is Firm firm)
            BeginEditFirm(firm);
    }

    internal void RowDelete_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is Firm firm)
            DeleteFirm(firm);
    }

    internal void BeginEditFirm(Firm firm)
    {
        Context.State.SuppressDirtyTracking = true;
        _editingFirm = firm;
        View<System.Windows.Controls.TextBox>("FirmNameBox").Text = firm.Name;
        View<System.Windows.Controls.TextBox>("TaxNoBox").Text = firm.TaxNo;
        View<System.Windows.Controls.TextBox>("ContactNameBox").Text = firm.ContactName;
        View<System.Windows.Controls.TextBox>("PhoneBox").Text = PhoneForEntry(firm.Phone);
        View<System.Windows.Controls.TextBox>("EmailBox").Text = firm.Email;
        View<System.Windows.Controls.TextBox>("AddressBox").Text = firm.Address;
        Context.Session.FirmEditorDirty = false;
        Context.State.SuppressDirtyTracking = false;
        View<System.Windows.Controls.Button>("FirmActionButton").Content = "KAYDET";
        View<System.Windows.Controls.Button>("FirmActionButton").ToolTip = "Düzenlenen firma bilgilerini kaydet";
        View<System.Windows.Controls.TextBox>("FirmNameBox").Focus();
    }

    internal void DeleteFirm(Firm firm)
    {
        var answer = MessageBox.Show($"'{firm.Name}' firmasını silmek istiyor musunuz?", "DoğruTemin", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (answer != MessageBoxResult.Yes)
            return;
        Context.State.Firms.Remove(firm);
        Context.Session.PreferredWinnerFirmId = string.Empty;
        ReNumberFirms();
        Context.Workspace.MarkDirty();
        if (_editingFirm == firm)
            ClearFirmForm();
    }

    internal static string PhoneForEntry(string? storedPhone)
    {
        var digits = new string ((storedPhone ?? string.Empty).Where(char.IsDigit).ToArray());
        return digits.Length == 11 && digits[0] == '0' ? digits[1..] : digits;
    }

    internal static string PhoneForStorage(string? enteredPhone)
    {
        var digits = new string ((enteredPhone ?? string.Empty).Where(char.IsDigit).ToArray());
        return digits.Length == 0 ? string.Empty : "0" + digits;
    }

    internal static bool IsValidPhoneEntry(string? enteredPhone)
    {
        var input = (enteredPhone ?? string.Empty).Trim();
        if (input.Length == 0)
            return true;
        if (input.Any(character => !char.IsDigit(character) && character is not (' ' or '-' or '(' or ')' or '.')))
            return false;
        return input.Count(char.IsDigit) == 10;
    }

    internal void ReNumberFirms()
    {
        for (var i = 0; i < Context.State.Firms.Count; i++)
            Context.State.Firms[i].No = i + 1;
        View<System.Windows.Controls.DataGrid>("FirmGrid").Items.Refresh();
        View<System.Windows.Controls.ComboBox>("OfferFirmSelector").Items.Refresh();
        if (Context.Offers._offerEditingFirm is not null && Context.State.Firms.Contains(Context.Offers._offerEditingFirm))
        {
            Context.Offers._suppressOfferSelection = true;
            View<System.Windows.Controls.ComboBox>("OfferFirmSelector").SelectedItem = Context.Offers._offerEditingFirm;
            Context.Offers._suppressOfferSelection = false;
        }
    }

    internal void ClearFirmForm()
    {
        var previousSuppress = Context.State.SuppressDirtyTracking;
        Context.State.SuppressDirtyTracking = true;
        _editingFirm = null;
        Context.Session.FirmEditorDirty = false;
        View<System.Windows.Controls.TextBox>("FirmNameBox").Clear();
        View<System.Windows.Controls.TextBox>("TaxNoBox").Clear();
        View<System.Windows.Controls.TextBox>("ContactNameBox").Clear();
        View<System.Windows.Controls.TextBox>("PhoneBox").Clear();
        View<System.Windows.Controls.TextBox>("EmailBox").Clear();
        View<System.Windows.Controls.TextBox>("AddressBox").Clear();
        View<System.Windows.Controls.DataGrid>("FirmGrid").SelectedItem = null;
        View<System.Windows.Controls.Button>("FirmActionButton").Content = "EKLE";
        View<System.Windows.Controls.Button>("FirmActionButton").ToolTip = "Yeni firma ekle";
        Context.State.SuppressDirtyTracking = previousSuppress;
    }

    internal void AddOrUpdateProduct_Click(object sender, RoutedEventArgs e)
    {
        var name = View<System.Windows.Controls.TextBox>("ProductNameBox").Text.Trim();
        if (name.Length == 0)
        {
            MessageBox.Show("Mal / hizmet adını giriniz.", "DoğruTemin", MessageBoxButton.OK, MessageBoxImage.Warning);
            View<System.Windows.Controls.TextBox>("ProductNameBox").Focus();
            return;
        }

        if (_editingProduct is null && Context.State.Products.Count >= ProcurementLimits.MaxProducts)
        {
            MessageBox.Show("En fazla 10 ürün / mal / hizmet eklenebilir.", "DoğruTemin", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var unit = (View<System.Windows.Controls.ComboBox>("ProductUnitBox").SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Adet";
        var quantity = View<System.Windows.Controls.TextBox>("ProductQuantityBox").Text.Trim();
        if (!double.TryParse(quantity.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var quantityValue) || quantityValue <= 0)
        {
            MessageBox.Show("Miktar sıfırdan büyük bir sayı olmalıdır.", "DoğruTemin", MessageBoxButton.OK, MessageBoxImage.Warning);
            View<System.Windows.Controls.TextBox>("ProductQuantityBox").Focus();
            return;
        }

        if (_editingProduct is null)
            Context.State.Products.Add(new ProductItem { No = Context.State.Products.Count + 1, Name = name, Specification = View<System.Windows.Controls.TextBox>("ProductSpecBox").Text.Trim(), Unit = unit, Quantity = quantity });
        else
        {
            _editingProduct.Name = name;
            _editingProduct.Specification = View<System.Windows.Controls.TextBox>("ProductSpecBox").Text.Trim();
            _editingProduct.Unit = unit;
            _editingProduct.Quantity = quantity;
            View<System.Windows.Controls.DataGrid>("ProductGrid").Items.Refresh();
        }

        Context.Session.PreferredWinnerFirmId = string.Empty;
        ReNumberProducts();
        View<System.Windows.Controls.TextBlock>("ProductCountText").Text = $"{Context.State.Products.Count} / 10 ürün";
        Context.Workspace.MarkDirty();
        ClearProductForm();
    }

    internal void ProductEdit_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not ProductItem item)
            return;
        Context.State.SuppressDirtyTracking = true;
        _editingProduct = item;
        View<System.Windows.Controls.TextBox>("ProductNameBox").Text = item.Name;
        View<System.Windows.Controls.TextBox>("ProductSpecBox").Text = item.Specification;
        var selectedUnit = View<System.Windows.Controls.ComboBox>("ProductUnitBox").Items.OfType<ComboBoxItem>().FirstOrDefault(option => option.Content?.ToString() == item.Unit);
        if (selectedUnit is null)
        {
            selectedUnit = new ComboBoxItem
            {
                Content = item.Unit
            };
            View<System.Windows.Controls.ComboBox>("ProductUnitBox").Items.Add(selectedUnit);
        }

        View<System.Windows.Controls.ComboBox>("ProductUnitBox").SelectedItem = selectedUnit;
        View<System.Windows.Controls.TextBox>("ProductQuantityBox").Text = item.Quantity;
        Context.Session.ProductEditorDirty = false;
        Context.State.SuppressDirtyTracking = false;
        View<System.Windows.Controls.Button>("ProductActionButton").Content = "KAYDET";
        View<System.Windows.Controls.TextBox>("ProductNameBox").Focus();
    }

    internal void ProductDelete_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is ProductItem item)
            DeleteProduct(item);
    }

    internal void DeleteProduct(ProductItem item)
    {
        var answer = MessageBox.Show($"'{item.Name}' kalemi silinsin mi?", "DoğruTemin", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (answer != MessageBoxResult.Yes)
            return;
        Context.State.Products.Remove(item);
        Context.Session.PreferredWinnerFirmId = string.Empty;
        foreach (var firm in Context.State.Firms)
            firm.Offers.RemoveAll(o => o.ProductId == item.Id);
        Context.Session.PreferredWinnerFirmId = string.Empty;
        ReNumberProducts();
        View<System.Windows.Controls.TextBlock>("ProductCountText").Text = $"{Context.State.Products.Count} / 10 ürün";
        if (_editingProduct == item)
            ClearProductForm();
        Context.Workspace.MarkDirty();
    }

    internal void ReNumberProducts()
    {
        for (var i = 0; i < Context.State.Products.Count; i++)
            Context.State.Products[i].No = i + 1;
        View<System.Windows.Controls.DataGrid>("ProductGrid").Items.Refresh();
    }

    internal void ClearProductForm()
    {
        var previousSuppress = Context.State.SuppressDirtyTracking;
        Context.State.SuppressDirtyTracking = true;
        _editingProduct = null;
        Context.Session.ProductEditorDirty = false;
        View<System.Windows.Controls.TextBox>("ProductNameBox").Clear();
        View<System.Windows.Controls.TextBox>("ProductSpecBox").Clear();
        View<System.Windows.Controls.ComboBox>("ProductUnitBox").SelectedIndex = 0;
        View<System.Windows.Controls.TextBox>("ProductQuantityBox").Text = "1";
        View<System.Windows.Controls.DataGrid>("ProductGrid").SelectedItem = null;
        View<System.Windows.Controls.Button>("ProductActionButton").Content = "EKLE";
        Context.State.SuppressDirtyTracking = previousSuppress;
    }
}
