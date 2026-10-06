namespace ShoeStore.Data;
public sealed record CartRow(int StockItemId, int ProductId, string Name, decimal Size, int Quantity, decimal UnitPrice)
{
    public decimal Total => Quantity * UnitPrice;
}
// Состояние текущего окна, без cookie/session. БД остаётся источником ролей, цен и остатков.
public sealed class StoreService(CatalogRepository catalog, OrderRepository orders, StoreClock clock)
{
    public StoreUser? Current { get; private set; }
    private readonly List<BasketLine> basket = [];
    public int BasketCount => basket.Sum(line => line.Quantity);
    public bool Staff => Current?.Role is "Admin" or "Manager";
    public bool Administrator => Current?.Role == "Admin";
    public DateTime Today => clock.Today;
    public async Task LoginAsync(string login)
    {
        if (string.IsNullOrWhiteSpace(login)) throw new ArgumentException("Введите логин");
        var user = await catalog.FindUserAsync(login) ?? throw new ArgumentException("Пользователь с таким логином не найден");
        Current = user; basket.Clear();
    }
    public void Logout() { Current = null; basket.Clear(); }
    private async Task<StoreUser> RequireAsync(bool staff = false, bool admin = false)
    {
        var current = Current;
        var fresh = current is null ? null : (await catalog.UsersAsync()).FirstOrDefault(user => user.Id == current.Id);
        if (fresh is null) { Logout(); throw new UnauthorizedAccessException("Для этого действия требуется вход"); }
        Current = fresh;
        if (admin && !Administrator || staff && !Staff) throw new UnauthorizedAccessException("Действие недоступно вашей роли");
        return fresh;
    }
    public Task<List<Product>> ProductsAsync() => catalog.ProductsAsync(Today);
    public Task<List<StockItem>> StockAsync(int productId) => catalog.StockAsync(productId);
    public async Task AddAsync(int productId, int stockId, int quantity)
    {
        await RequireAsync();
        var item = (await catalog.StockAsync(productId)).FirstOrDefault(item => item.Id == stockId);
        var existing = basket.FirstOrDefault(line => line.StockItemId == stockId)?.Quantity ?? 0;
        if (item is null || quantity < 1 || (long)quantity + existing > item.Available)
            throw new ArgumentException("Выберите доступный размер и целое количество от 1 до остатка с учётом корзины");
        basket.RemoveAll(line => line.StockItemId == stockId);
        basket.Add(new BasketLine(stockId, existing + quantity));
    }
    public async Task<List<CartRow>> CartAsync()
    {
        await RequireAsync();
        var rows = new List<CartRow>();
        foreach (var product in await ProductsAsync())
        foreach (var stock in await catalog.StockAsync(product.Id))
        {
            var line = basket.FirstOrDefault(line => line.StockItemId == stock.Id);
            if (line is not null) rows.Add(new CartRow(stock.Id, product.Id, product.Name, stock.Size, line.Quantity, product.FinalPrice));
        }
        if (rows.Count != basket.Count) throw new ArgumentException("Состав каталога изменился. Отмените корзину и выберите товары заново");
        return rows;
    }
    public async Task QuantityAsync(int stockId, int quantity)
    {
        await RequireAsync();
        if (quantity < 0) throw new ArgumentException("Количество не может быть отрицательным");
        var row = (await CartAsync()).FirstOrDefault(row => row.StockItemId == stockId) ?? throw new ArgumentException("Строка корзины не найдена");
        var item = (await catalog.StockAsync(row.ProductId)).FirstOrDefault(item => item.Id == stockId);
        if (item is null || quantity > item.Available) throw new ArgumentException("Количество превышает доступный остаток. Обновите каталог");
        // Подтверждение ещё раз проверит остаток условным UPDATE в транзакции.
        basket.RemoveAll(line => line.StockItemId == row.StockItemId);
        if (quantity > 0) basket.Add(new BasketLine(stockId, quantity));
    }
    public void Cancel() => basket.Clear();
    public async Task<List<StoreUser>> CustomersAsync() { await RequireAsync(staff: true); return await catalog.UsersAsync(); }
    public async Task<int> ConfirmAsync(int? customerId)
    {
        var user = await RequireAsync();
        var id = Staff && customerId.HasValue ? customerId.Value : user.Id;
        if (!(await catalog.UsersAsync()).Any(customer => customer.Id == id)) throw new ArgumentException("Выберите существующего клиента");
        var orderId = await orders.CreateAsync(id, basket.ToList(), Today);
        basket.Clear(); return orderId;
    }
    public async Task<List<OrderHeader>> OrdersAsync() { await RequireAsync(staff: true); return await orders.AllAsync(); }
    public async Task<List<OrderLine>> LinesAsync(int id) { await RequireAsync(staff: true); return await orders.LinesAsync(id); }
    public async Task ChangeDateAsync(int id, DateTime date) { await RequireAsync(admin: true); await orders.ChangeDateAsync(id, date); }
    public async Task RemoveOrderAsync(int id) { await RequireAsync(staff: true); await orders.RemoveAsync(id); }
    public async Task RemoveLineAsync(int id, int lineId) { await RequireAsync(admin: true); await orders.RemoveAsync(id, lineId); }
}
