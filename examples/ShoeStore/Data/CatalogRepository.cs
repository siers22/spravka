namespace ShoeStore.Data;

public sealed class CatalogRepository(Database database)
{
    public async Task<StoreUser?> FindUserAsync(string login)
    {
        await using var connection=database.CreateConnection();
        await connection.OpenAsync();
        await using var command=Database.Command(connection,
            "SELECT id,login,last_name,first_name,patronymic,role FROM users WHERE login=@login",("@login",login.Trim()));
        await using var reader=await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? new StoreUser(reader.GetInt32(0),reader.GetString(1),
            $"{reader.GetString(2)} {reader.GetString(3)} {reader.GetString(4)}",reader.GetString(5)) : null;
    }
    public async Task<List<StoreUser>> UsersAsync()
    {
        await using var connection=database.CreateConnection();
        await connection.OpenAsync();
        await using var command=Database.Command(connection,"SELECT id,login,last_name,first_name,patronymic,role FROM users ORDER BY id");
        await using var reader=await command.ExecuteReaderAsync();
        var result=new List<StoreUser>();
        while (await reader.ReadAsync()) result.Add(new StoreUser(reader.GetInt32(0),reader.GetString(1),
            $"{reader.GetString(2)} {reader.GetString(3)} {reader.GetString(4)}",reader.GetString(5)));
        return result;
    }
    public async Task<List<Product>> ProductsAsync(DateTime date)
    {
        var (previousMonthStart,currentMonthStart)=DiscountCalculator.Period(date);
        await using var connection=database.CreateConnection();
        await connection.OpenAsync();
        await using var history=Database.Command(connection,"""
            SELECT DISTINCT s.product_id FROM order_items oi
            JOIN orders o ON o.id=oi.order_id JOIN stock_items s ON s.id=oi.stock_item_id
            WHERE o.order_date>=@start AND o.order_date<@end
            """,("@start",previousMonthStart),("@end",currentMonthStart));
        var ordered=new HashSet<int>();
        await using (var reader=await history.ExecuteReaderAsync())
            while (await reader.ReadAsync()) ordered.Add(reader.GetInt32(0));
        await using var command=Database.Command(connection,"""
            SELECT p.id,p.name,c.name,sc.name,m.name,p.image_path,p.description,p.composition,p.base_price,
             COALESCE((SELECT SUM(s.available_quantity) FROM stock_items s WHERE s.product_id=p.id),0)
            FROM products p JOIN subcategories sc ON sc.id=p.subcategory_id
            JOIN categories c ON c.id=sc.category_id JOIN manufacturers m ON m.id=p.manufacturer_id
            ORDER BY p.id
            """);
        await using var rows=await command.ExecuteReaderAsync();
        var result=new List<Product>();
        while (await rows.ReadAsync())
        {
            var id=rows.GetInt32(0);
            var price=rows.GetDecimal(8);
            result.Add(new Product(id,rows.GetString(1),rows.GetString(2),rows.GetString(3),rows.GetString(4),
                rows.GetString(5),rows.GetString(6),rows.GetString(7),price,
                DiscountCalculator.Price(price,ordered.Contains(id)),rows.GetInt32(9)));
        }
        return result;
    }
    public async Task<List<StockItem>> StockAsync(int productId)
    {
        await using var connection=database.CreateConnection();
        await connection.OpenAsync();
        await using var command=Database.Command(connection,
            "SELECT id,product_id,size,available_quantity FROM stock_items WHERE product_id=@id ORDER BY size",("@id",productId));
        await using var reader=await command.ExecuteReaderAsync();
        var result=new List<StockItem>();
        while (await reader.ReadAsync()) result.Add(new StockItem(reader.GetInt32(0),reader.GetInt32(1),reader.GetDecimal(2),reader.GetInt32(3)));
        return result;
    }
}
