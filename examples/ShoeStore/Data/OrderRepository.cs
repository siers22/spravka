using System.Data;
using System.Data.Common;
namespace ShoeStore.Data;

public sealed class OrderRepository(Database database)
{
    public async Task<int> CreateAsync(int userId,List<BasketLine> basket,DateTime date)
    {
        if (basket.Count==0 || basket.Any(line=>line.Quantity<1)) throw new ArgumentException("Выберите товары и положительное количество");
        var lines=basket.GroupBy(line=>line.StockItemId)
            .Select(group=>new BasketLine(group.Key,checked(group.Sum(line=>line.Quantity)))).OrderBy(line=>line.StockItemId).ToList();
        await using var connection=database.CreateConnection();
        await connection.OpenAsync();
        await using var transaction=await connection.BeginTransactionAsync(IsolationLevel.Serializable);
        async Task<object?> Scalar(string sql,params (string,object?)[] values)
        {
            await using var command=Database.Command(connection,sql,values);
            command.Transaction=transaction;
            return await command.ExecuteScalarAsync();
        }
        async Task<int> Execute(string sql,params (string,object?)[] values)
        {
            await using var command=Database.Command(connection,sql,values);
            command.Transaction=transaction;
            return await command.ExecuteNonQueryAsync();
        }
        // Остаток проверяется условным UPDATE внутри той же транзакции, что и заказ.
        // Все модели оцениваются ДО вставки нового заказа, чтобы он не менял историю скидки.
        var priced=new List<(BasketLine Line,decimal Price)>();
        var (start,end)=DiscountCalculator.Period(date);
        foreach (var line in lines)
        {
            var model=await Scalar("SELECT product_id FROM stock_items WHERE id=@id",("@id",line.StockItemId));
            if (model is null) throw new ArgumentException("Товарная позиция не найдена");
            var price=Convert.ToDecimal(await Scalar("SELECT base_price FROM products WHERE id=@id",("@id",model)));
            var count=Convert.ToInt32(await Scalar("""
                SELECT COUNT(*) FROM order_items oi JOIN orders o ON o.id=oi.order_id
                JOIN stock_items s ON s.id=oi.stock_item_id
                WHERE s.product_id=@product AND o.order_date>=@start AND o.order_date<@end
                """,("@product",model),("@start",start),("@end",end)));
            if (await Execute("UPDATE stock_items SET available_quantity=available_quantity-@quantity WHERE id=@id AND available_quantity>=@quantity",
                ("@quantity",line.Quantity),("@id",line.StockItemId))!=1)
                throw new ArgumentException("Доступное количество изменилось. Обновите каталог и уменьшите количество");
            priced.Add((line,DiscountCalculator.Price(price,count>0)));
        }
        var orderId=Convert.ToInt32(await Scalar("SELECT COALESCE(MAX(id),0)+1 FROM orders"));
        await Execute("INSERT INTO orders VALUES(@id,@date,@user)",("@id",orderId),("@date",date.Date),("@user",userId));
        var itemId=Convert.ToInt32(await Scalar("SELECT COALESCE(MAX(id),0)+1 FROM order_items"));
        foreach (var row in priced)
            await Execute("INSERT INTO order_items VALUES(@id,@order,@sku,@quantity,@price)",
                ("@id",itemId++),("@order",orderId),("@sku",row.Line.StockItemId),
                ("@quantity",row.Line.Quantity),("@price",row.Price));
        await transaction.CommitAsync();
        return orderId;
    }
    public async Task<List<OrderHeader>> AllAsync()
    {
        await using var connection=database.CreateConnection();
        await connection.OpenAsync();
        await using var command=Database.Command(connection,"""
            SELECT o.id,o.order_date,u.last_name,u.first_name,u.patronymic,
              COALESCE((SELECT SUM(oi.quantity*oi.unit_price) FROM order_items oi WHERE oi.order_id=o.id),0)
            FROM orders o JOIN users u ON u.id=o.user_id ORDER BY o.order_date DESC,o.id DESC
            """);
        await using var reader=await command.ExecuteReaderAsync();
        var result=new List<OrderHeader>();
        while (await reader.ReadAsync()) result.Add(new OrderHeader(reader.GetInt32(0),reader.GetDateTime(1),
            $"{reader.GetString(2)} {reader.GetString(3)} {reader.GetString(4)}",reader.GetDecimal(5)));
        return result;
    }
    public async Task<List<OrderLine>> LinesAsync(int orderId)
    {
        await using var connection=database.CreateConnection();
        await connection.OpenAsync();
        await using var command=Database.Command(connection,"""
            SELECT oi.id,oi.stock_item_id,p.name,m.name,s.size,oi.quantity,oi.unit_price
            FROM order_items oi JOIN stock_items s ON s.id=oi.stock_item_id
            JOIN products p ON p.id=s.product_id JOIN manufacturers m ON m.id=p.manufacturer_id
            WHERE oi.order_id=@id ORDER BY oi.id
            """,("@id",orderId));
        await using var reader=await command.ExecuteReaderAsync();
        var result=new List<OrderLine>();
        while (await reader.ReadAsync()) result.Add(new OrderLine(reader.GetInt32(0),reader.GetInt32(1),reader.GetString(2),
            reader.GetString(3),reader.GetDecimal(4),reader.GetInt32(5),reader.GetDecimal(6)));
        return result;
    }
    public async Task ChangeDateAsync(int orderId,DateTime date)
    {
        if (date==default) throw new ArgumentException("Укажите корректную дату");
        await using var connection=database.CreateConnection();
        await connection.OpenAsync();
        await using var command=Database.Command(connection,"UPDATE orders SET order_date=@date WHERE id=@id",("@date",date.Date),("@id",orderId));
        if (await command.ExecuteNonQueryAsync()!=1) throw new ArgumentException("Заказ не найден");
    }
    public async Task RemoveAsync(int orderId,int? itemId=null)
    {
        await using var connection=database.CreateConnection();
        await connection.OpenAsync();
        await using var transaction=await connection.BeginTransactionAsync(IsolationLevel.Serializable);
        var parameters=new List<(string,object?)>{("@order",orderId)};
        var condition="order_id=@order";
        if (itemId.HasValue) { condition+=" AND id=@item";parameters.Add(("@item",itemId.Value)); }
        await using var read=Database.Command(connection,"SELECT stock_item_id,quantity FROM order_items WHERE "+condition,parameters.ToArray());
        read.Transaction=transaction;
        var restore=new List<(int Id,int Quantity)>();
        await using (var reader=await read.ExecuteReaderAsync())
            while (await reader.ReadAsync()) restore.Add((reader.GetInt32(0),reader.GetInt32(1)));
        if (restore.Count==0) throw new ArgumentException("Заказ или позиция уже удалены. Обновите список");
        foreach (var row in restore)
        {
            await using var update=Database.Command(connection,
                "UPDATE stock_items SET available_quantity=available_quantity+@quantity WHERE id=@id",
                ("@quantity",row.Quantity),("@id",row.Id));
            update.Transaction=transaction;
            await update.ExecuteNonQueryAsync();
        }
        await using var delete=Database.Command(connection,"DELETE FROM order_items WHERE "+condition,parameters.ToArray());
        delete.Transaction=transaction;
        await delete.ExecuteNonQueryAsync();
        await using var deleteHeader=Database.Command(connection,"DELETE FROM orders WHERE id=@order AND NOT EXISTS(SELECT 1 FROM order_items WHERE order_id=@order)",("@order",orderId));
        deleteHeader.Transaction=transaction;
        await deleteHeader.ExecuteNonQueryAsync();
        await transaction.CommitAsync();
    }
}
