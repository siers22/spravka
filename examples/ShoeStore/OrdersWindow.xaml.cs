using System.Windows;
using System.Windows.Controls;
using ShoeStore.Data;
namespace ShoeStore;
public partial class OrdersWindow : Window
{
    private bool busy;
    public OrdersWindow() { InitializeComponent(); Closing += (_, e) => { if (busy) e.Cancel = true; }; Loaded += async (_, _) => await LoadAsync(); }
    private async Task LoadAsync()
    {
        try { Headers.ItemsSource = await App.Store.OrdersAsync(); Lines.ItemsSource = null; Total.Text = "Выберите заказ, чтобы увидеть состав"; AdminControls.Visibility = App.Store.Administrator ? Visibility.Visible : Visibility.Collapsed; }
        catch (Exception ex) { App.Error(this, ex); }
    }
    private async void OrderSelected(object sender, SelectionChangedEventArgs e)
    {
        if (Headers.SelectedItem is not OrderHeader order) return;
        try
        {
            var lines = await App.Store.LinesAsync(order.Id);
            if (Headers.SelectedItem is not OrderHeader current || current.Id != order.Id) return;
            Lines.ItemsSource = lines; Date.SelectedDate = order.Date; Total.Text = $"Заказ № {order.Id} · Итого: {lines.Sum(line => line.Total):N2} ₽";
        }
        catch (Exception ex) { App.Error(this, ex); }
    }
    private async Task RunAsync(Func<int, Task> action)
    {
        if (busy) return; busy = true;
        try { if (Headers.SelectedItem is not OrderHeader order) throw new ArgumentException("Выберите заказ"); await action(order.Id); await LoadAsync(); }
        catch (Exception ex) { App.Error(this, ex); }
        finally { busy = false; }
    }
    private async void Date_Click(object sender, RoutedEventArgs e) => await RunAsync(id => Date.SelectedDate is DateTime date ? App.Store.ChangeDateAsync(id, date) : throw new ArgumentException("Укажите корректную дату"));
    private async void Line_Click(object sender, RoutedEventArgs e)
    {
        if (Lines.SelectedItem is not OrderLine line) { App.Error(this, new ArgumentException("Выберите строку заказа")); return; }
        if (MessageBox.Show(this, "Удалить строку и вернуть её количество на склад?", "Чудо Обувь · Удаление", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            await RunAsync(id => App.Store.RemoveLineAsync(id, line.Id));
    }
    private async void Remove_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show(this, "Удалить заказ целиком и вернуть все пары на склад?", "Чудо Обувь · Удаление", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            await RunAsync(App.Store.RemoveOrderAsync);
    }
    private async void Refresh_Click(object sender, RoutedEventArgs e) => await LoadAsync();
    private void Back_Click(object sender, RoutedEventArgs e) => Close();
}
