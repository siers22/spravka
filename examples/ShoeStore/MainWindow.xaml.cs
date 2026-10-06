using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ShoeStore.Data;
namespace ShoeStore;
public partial class MainWindow : Window
{
    private List<Product> all = [];
    private bool ready;
    private bool busy;
    public MainWindow()
    {
        InitializeComponent(); Closing += (_, e) => { if (busy) e.Cancel = true; };
        Logo.Source = new BitmapImage(new Uri(Path.Combine(AppContext.BaseDirectory, "Assets", "logo.png")));
        Loaded += async (_, _) => await LoadAsync();
    }
    private async Task LoadAsync()
    {
        if (busy) return;
        busy = true; Import.IsEnabled = false;
        try
        {
            all = await App.Store.ProductsAsync();
            var category = Category.SelectedItem as string;
            ready = false;
            Category.ItemsSource = new[] { "Все категории" }.Concat(all.Select(p => p.Category).Distinct().Order()).ToList();
            Category.SelectedItem = category is not null && Category.Items.Contains(category) ? category : "Все категории";
            ready = true; RefreshIdentity(); ApplyFilters();
        }
        catch (Exception ex) { App.Error(this, ex); }
        finally { busy = false; Import.IsEnabled = true; }
    }
    private void RefreshIdentity()
    {
        Identity.Text = App.Store.Current is null ? "Гость" : $"{App.Store.Current.FullName} · {App.Store.Current.Role}";
        Filters.IsEnabled = Cart.IsEnabled = App.Store.Current is not null;
        Orders.Visibility = App.Store.Staff ? Visibility.Visible : Visibility.Collapsed;
        Cart.Content = $"Корзина ({App.Store.BasketCount})";
    }
    private void ApplyFilters()
    {
        if (!ready) return;
        IEnumerable<Product> selected = all;
        if (App.Store.Current is not null)
        {
            var query = Search.Text.Trim();
            if (query.Length > 0) selected = selected.Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || p.Description.Contains(query, StringComparison.OrdinalIgnoreCase));
            if (Category.SelectedItem is string category && category != "Все категории") selected = selected.Where(p => p.Category == category);
            selected = Sort.SelectedIndex == 1 ? selected.OrderByDescending(p => p.FinalPrice).ThenBy(p => p.Id) : selected.OrderBy(p => p.FinalPrice).ThenBy(p => p.Id);
        }
        var rows = selected.Select(p => new ProductRow(p)).ToList(); Products.ItemsSource = rows;
        Count.Text = rows.Count == 0 ? "Ничего не найдено. Измените поиск или категорию." : $"Найдено {rows.Count} из {all.Count} · Дата расчёта: {App.Store.Today:dd.MM.yyyy}";
    }
    private void FilterChanged(object sender, RoutedEventArgs e) => ApplyFilters();
    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        if (App.Store.Current is null) new LoginWindow { Owner = this }.ShowDialog();
        else { App.Store.Logout(); Search.Clear(); }
        RefreshIdentity(); await LoadAsync();
    }
    private void Product_Click(object sender, MouseButtonEventArgs e)
    {
        if (Products.SelectedItem is not ProductRow row) return;
        new ProductWindow(row.Product) { Owner = this }.ShowDialog(); RefreshIdentity();
    }
    private async void Cart_Click(object sender, RoutedEventArgs e)
    {
        if (App.Store.Current is null) return;
        new CartWindow { Owner = this }.ShowDialog(); RefreshIdentity(); await LoadAsync();
    }
    private async void Orders_Click(object sender, RoutedEventArgs e)
    {
        try { await App.Store.OrdersAsync(); new OrdersWindow { Owner = this }.ShowDialog(); await LoadAsync(); }
        catch (Exception ex) { App.Error(this, ex); }
    }
    private async void Import_Click(object sender, RoutedEventArgs e)
    {
        if (busy) return;
        busy = true; Import.IsEnabled = false;
        try { await App.Importer.RunAsync(); MessageBox.Show(this, "Импортированы 31 модель, 92 позиции, 35 размеров, 20 пользователей и 10 заказов.", "Чудо Обувь · Импорт готов", MessageBoxButton.OK, MessageBoxImage.Information); }
        catch (Exception ex) { App.Error(this, ex); }
        finally { busy = false; Import.IsEnabled = true; }
        await LoadAsync();
    }
    private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadAsync();
}
