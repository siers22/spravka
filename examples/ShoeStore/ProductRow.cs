using System.Globalization;
using System.IO;
using System.Windows.Data;
namespace ShoeStore;
public sealed record ProductRow(Data.Product Product)
{
    public string Caption => $"{Product.Category} · {Product.Subcategory} · {Product.Manufacturer}";
    public string Price => $"{Product.FinalPrice:N2} ₽";
    public string Stock => $"Остаток: {Product.Available} · {(Product.Available > 5 ? "много" : "мало")}";
    public string Background => Product.Available <= 3 ? "#ff8080" : "#FFFFFF";
    public string Discount => Product.FinalPrice < Product.BasePrice ? "Скидка 25%" : "Без скидки";
}
public sealed class ProductImageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var name = Path.GetFileName((value?.ToString() ?? "").Replace('\\', '/'));
        var image = Path.Combine(AppContext.BaseDirectory, "Assets", "images", name);
        return File.Exists(image) ? image : Path.Combine(AppContext.BaseDirectory, "Assets", "picture.png");
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}
