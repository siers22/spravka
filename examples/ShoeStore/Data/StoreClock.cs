using System.Globalization;
namespace ShoeStore.Data;
public sealed class StoreClock(IConfiguration configuration)
{
    // Только для воспроизводимой тренировки на заказах апреля/мая 2026.
    // Пустое значение использует текущую дату Windows.
    public DateTime Today => DateTime.TryParseExact(configuration["CalculationDate"],"yyyy-MM-dd",
        CultureInfo.InvariantCulture,DateTimeStyles.None,out var date) ? date.Date : DateTime.Today;
}
