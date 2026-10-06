using System.Globalization;
using System.Windows;
using System.Windows.Media.Imaging;
using ShoeStore.Data;
namespace ShoeStore;
public partial class ProductWindow : Window
{
    private readonly Product product;
    private bool busy;
    private sealed record Choice(StockItem Item, string Label);
    public ProductWindow(Product product)
    {
        this.product = product; InitializeComponent(); Closing += (_, e) => { if (busy) e.Cancel = true; }; NameLabel.Text = product.Name;
        Details.Text = $"{product.Category} · {product.Subcategory}\nПроизводитель: {product.Manufacturer}\nСостав: {product.Composition}\n{product.Description}\nЦена: {product.FinalPrice:N2} ₽";
        Photo.Source = new BitmapImage(new Uri((string)new ProductImageConverter().Convert(product.ImagePath, typeof(string), "", CultureInfo.InvariantCulture)));
        Add.IsEnabled = false; Loaded += async (_, _) => await LoadAsync();
    }
    private async Task LoadAsync()
    {
        try
        {
            var items = await App.Store.StockAsync(product.Id);
            Sizes.Text = "Все размеры модели: " + string.Join(", ", items.Select(item => item.Size.ToString(CultureInfo.CurrentCulture)));
            Stock.ItemsSource = items.Where(item => item.Available > 0).Select(item => new Choice(item, $"{item.Size} · доступно {item.Available}")).ToList();
            Stock.SelectedIndex = 0; Add.IsEnabled = App.Store.Current is not null && Stock.Items.Count > 0;
            if (App.Store.Current is null) Add.Content = "Для заказа войдите в каталог";
        }
        catch (Exception ex) { App.Error(this, ex); }
    }
    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        busy = true; Add.IsEnabled = false;
        try
        {
            if (Stock.SelectedItem is not Choice choice || !int.TryParse(Quantity.Text, NumberStyles.None, CultureInfo.InvariantCulture, out var quantity)) throw new ArgumentException("Выберите размер и введите целое положительное количество");
            await App.Store.AddAsync(product.Id, choice.Item.Id, quantity);
            MessageBox.Show(this, "Товар добавлен. Продолжите выбор или подтвердите заказ в корзине.", "Чудо Обувь · Корзина", MessageBoxButton.OK, MessageBoxImage.Information); busy = false; Close();
        }
        catch (Exception ex) { App.Error(this, ex); }
        finally { busy = false; Add.IsEnabled = true; }
    }
    private void Back_Click(object sender, RoutedEventArgs e) => Close();
}
