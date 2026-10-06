namespace ShoeStore.Data;

public sealed record StoreUser(int Id,string Login,string FullName,string Role);
public sealed record Product(int Id,string Name,string Category,string Subcategory,string Manufacturer,
    string ImagePath,string Description,string Composition,decimal BasePrice,decimal FinalPrice,int Available);
public sealed record StockItem(int Id,int ProductId,decimal Size,int Available);
public sealed record BasketLine(int StockItemId,int Quantity);
public sealed record OrderHeader(int Id,DateTime Date,string FullName,decimal Total);
public sealed record OrderLine(int Id,int StockItemId,string Name,string Manufacturer,decimal Size,int Quantity,decimal UnitPrice)
{ public decimal Total => Quantity * UnitPrice; }

public static class DiscountCalculator
{
    public static (DateTime PreviousMonthStart,DateTime CurrentMonthStart) Period(DateTime date)
    {
        var currentMonthStart = new DateTime(date.Year,date.Month,1);
        return (currentMonthStart.AddMonths(-1),currentMonthStart);
    }
    public static decimal Price(decimal basePrice,bool orderedLastMonth) =>
        orderedLastMonth ? basePrice : decimal.Round(basePrice*0.75m,2,MidpointRounding.AwayFromZero);
}
