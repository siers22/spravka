using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using ShoeStore.Data;
namespace ShoeStore;
public partial class CartWindow : Window
{
    private bool busy;
    public CartWindow() { InitializeComponent(); Closing += (_, e) => { if (busy) e.Cancel = true; }; Loaded += async (_, _) => await LoadAsync(); }
    private async Task LoadAsync()
    {
        try
        {
            var rows = await App.Store.CartAsync(); Rows.ItemsSource = rows; Total.Text = $"Итого: {rows.Sum(row => row.Total):N2} ₽";
            Confirm.IsEnabled = rows.Count > 0;
            Customer.Visibility = CustomerLabel.Visibility = App.Store.Staff ? Visibility.Visible : Visibility.Collapsed;
            if (App.Store.Staff && Customer.ItemsSource is null) { Customer.ItemsSource = await App.Store.CustomersAsync(); Customer.SelectedItem = ((List<StoreUser>)Customer.ItemsSource).FirstOrDefault(user => user.Id == App.Store.Current?.Id); }
        }
        catch (Exception ex) { Confirm.IsEnabled = false; App.Error(this, ex); }
    }
    private void SelectionChanged(object sender, SelectionChangedEventArgs e) { if (Rows.SelectedItem is CartRow row) Quantity.Text = row.Quantity.ToString(CultureInfo.InvariantCulture); }
    private async void Quantity_Click(object sender, RoutedEventArgs e)
    {
        if (busy) return; busy = true;
        try
        {
            if (Rows.SelectedItem is not CartRow row || !int.TryParse(Quantity.Text, NumberStyles.None, CultureInfo.InvariantCulture, out var quantity)) throw new ArgumentException("Выберите строку и целое неотрицательное количество");
            await App.Store.QuantityAsync(row.StockItemId, quantity); await LoadAsync();
        }
        catch (Exception ex) { App.Error(this, ex); }
        finally { busy = false; }
    }
    private async void Confirm_Click(object sender, RoutedEventArgs e)
    {
        if (busy) return;
        if (MessageBox.Show(this, "Сохранить заказ и списать выбранное количество со склада?", "Чудо Обувь · Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        busy = true; Confirm.IsEnabled = false;
        try
        {
            if (App.Store.Staff && Customer.SelectedItem is not StoreUser) throw new ArgumentException("Выберите клиента");
            var id = await App.Store.ConfirmAsync((Customer.SelectedItem as StoreUser)?.Id);
            MessageBox.Show(this, $"Заказ № {id} оформлен. Остатки обновлены.", "Чудо Обувь · Готово", MessageBoxButton.OK, MessageBoxImage.Information); busy = false; Close();
        }
        catch (Exception ex) { App.Error(this, ex); await LoadAsync(); }
        finally { busy = false; }
    }
    private void Cancel_Click(object sender, RoutedEventArgs e) { if (busy) return; App.Store.Cancel(); Close(); }
    private void Back_Click(object sender, RoutedEventArgs e) { if (!busy) Close(); }
}
