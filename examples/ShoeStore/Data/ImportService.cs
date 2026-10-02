using System.Data.Common;
using System.Globalization;
using System.Text.RegularExpressions;
using ClosedXML.Excel;

namespace ShoeStore.Data;

public sealed class ImportService(Database database, IWebHostEnvironment environment)
{
    public static string Normalize(string text) => Regex.Replace(text.Replace('\u00a0',' '), @"\s+", " ").Trim();
    // Явное сопоставление сокращённого названия с каталогом. Это решение для данного архива.
    private static string ProductName(string text) => Normalize(text) switch
    {
        "Черные туфли в классическом стиле" => "Черные туфли в классическом стиле — база для деловых образов",
        var name => name
    };
    private static decimal Size(IXLCell cell) => decimal.Parse(cell.GetString().Replace(',','.'),CultureInfo.InvariantCulture);
    private List<IXLRow> Rows(string name, out XLWorkbook workbook)
    {
        workbook = new XLWorkbook(Path.Combine(environment.ContentRootPath,"Data",name));
        return workbook.Worksheet(1).RowsUsed().Skip(1).ToList();
    }
    public async Task RunAsync()
    {
        await using var connection = database.CreateConnection();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await using var count = Database.Command(connection,"SELECT COUNT(*) FROM products");
        count.Transaction = transaction;
        if (Convert.ToInt32(await count.ExecuteScalarAsync()) != 0)
            throw new InvalidOperationException("Импорт выполняется в пустую базу. Не запускайте его повторно");
        async Task Insert(string sql, params (string,object?)[] parameters)
        {
            await using var command = Database.Command(connection,sql,parameters);
            command.Transaction = transaction;
            await command.ExecuteNonQueryAsync();
        }
        var categories = new Dictionary<string,int>();
        var subcategories = new Dictionary<(int,string),int>();
        var manufacturers = new Dictionary<string,int>();
        var products = new Dictionary<(string,string),int>();
        var people = new Dictionary<string,int>();
        var stock = new Dictionary<(int,decimal),int>();
        var rows = Rows("Users_import.xlsx",out var usersBook);
        using (usersBook)
        foreach (var row in rows)
        {
            var id = people.Count+1;
            var last = Normalize(row.Cell(1).GetString());
            var first = Normalize(row.Cell(2).GetString());
            var patronymic = Normalize(row.Cell(3).GetString());
            people.Add($"{last} {first} {patronymic}",id);
            var role = Normalize(row.Cell(5).GetString()) switch
            {
                "Администратор" => "Admin",
                "Менеджер" => "Manager",
                "Авторизованный пользователь" => "User",
                _ => throw new InvalidOperationException("Неизвестная роль в строке "+row.RowNumber())
            };
            await Insert("INSERT INTO users VALUES(@id,@login,@last,@first,@patronymic,@role)",
                ("@id",id),("@login",Normalize(row.Cell(4).GetString())),("@last",last),
                ("@first",first),("@patronymic",patronymic),("@role",role));
        }
        rows = Rows("Sizes_import.xlsx",out var sizesBook);
        using (sizesBook)
        foreach (var row in rows) await Insert("INSERT INTO sizes VALUES(@size)",("@size",Size(row.Cell(1))));
        rows = Rows("Products_import.xlsx",out var productsBook);
        using (productsBook)
        foreach (var row in rows)
        {
            var category = Normalize(row.Cell(1).GetString());
            if (!categories.TryGetValue(category,out var categoryId))
            {
                categoryId=categories.Count+1;
                categories.Add(category,categoryId);
                await Insert("INSERT INTO categories VALUES(@id,@name)",("@id",categoryId),("@name",category));
            }
            var subcategory = Normalize(row.Cell(2).GetString());
            if (!subcategories.TryGetValue((categoryId,subcategory),out var subcategoryId))
            {
                subcategoryId=subcategories.Count+1;
                subcategories.Add((categoryId,subcategory),subcategoryId);
                await Insert("INSERT INTO subcategories VALUES(@id,@category,@name)",
                    ("@id",subcategoryId),("@category",categoryId),("@name",subcategory));
            }
            var manufacturer = Normalize(row.Cell(5).GetString());
            if (!manufacturers.TryGetValue(manufacturer,out var manufacturerId))
            {
                manufacturerId=manufacturers.Count+1;
                manufacturers.Add(manufacturer,manufacturerId);
                await Insert("INSERT INTO manufacturers VALUES(@id,@name)",("@id",manufacturerId),("@name",manufacturer));
            }
            var id = products.Count+1;
            var name = ProductName(row.Cell(4).GetString());
            products.Add((name,manufacturer),id);
            await Insert("INSERT INTO products VALUES(@id,@subcategory,@manufacturer,@name,@image,@description,@composition,@price)",
                ("@id",id),("@subcategory",subcategoryId),("@manufacturer",manufacturerId),
                ("@name",name),("@image",Normalize(row.Cell(3).GetString())),
                ("@description",row.Cell(6).GetString()),("@composition",row.Cell(7).GetString()),
                ("@price",row.Cell(8).GetValue<decimal>()));
        }
        rows = Rows("Stock_Items_import.xlsx",out var stockBook);
        using (stockBook)
        foreach (var row in rows)
        {
            var key = (ProductName(row.Cell(1).GetString()),Normalize(row.Cell(2).GetString()));
            if (!products.TryGetValue(key,out var productId))
                throw new InvalidOperationException($"Нет модели для складской строки {row.RowNumber()}: {key}");
            var size = Size(row.Cell(3));
            var id = stock.Count+1;
            stock.Add((productId,size),id);
            // XLSX уже содержит доступный остаток. Исторические заказы НЕ вычитаем второй раз.
            await Insert("INSERT INTO stock_items VALUES(@id,@product,@size,@quantity)",
                ("@id",id),("@product",productId),("@size",size),("@quantity",row.Cell(4).GetValue<int>()));
        }
        var orders = new Dictionary<int,(DateTime,string)>();
        var lineId = 0;
        rows = Rows("Orders_import.xlsx",out var ordersBook);
        using (ordersBook)
        foreach (var row in rows)
        {
            var orderId=row.Cell(1).GetValue<int>();
            var date=row.Cell(2).GetDateTime().Date;
            var fullName=Normalize(row.Cell(3).GetString());
            if (!people.TryGetValue(fullName,out var userId)) throw new InvalidOperationException("Не найден клиент "+fullName);
            if (!orders.TryGetValue(orderId,out var header))
            {
                orders.Add(orderId,(date,fullName));
                await Insert("INSERT INTO orders VALUES(@id,@date,@user)",("@id",orderId),("@date",date),("@user",userId));
            }
            else if (header!=(date,fullName)) throw new InvalidOperationException("Противоречивая шапка заказа "+orderId);
            var productKey=(ProductName(row.Cell(5).GetString()),Normalize(row.Cell(6).GetString()));
            var sku=stock[(products[productKey],Size(row.Cell(7)))];
            await Insert("INSERT INTO order_items VALUES(@id,@order,@sku,@quantity,@price)",
                ("@id",++lineId),("@order",orderId),("@sku",sku),
                ("@quantity",row.Cell(8).GetValue<int>()),("@price",row.Cell(9).GetValue<decimal>()));
        }
        await transaction.CommitAsync();
        Console.WriteLine($"Импортировано: {products.Count} модель, {stock.Count} позиций, {people.Count} пользователей, {orders.Count} заказов, {lineId} строк заказа.");
    }
}
