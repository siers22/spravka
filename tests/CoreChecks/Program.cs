using Microsoft.Extensions.Configuration;
using Npgsql;
using ExamGuide.Data;
using ShoeStore.Data;
using EDatabase = ExamGuide.Data.Database;
using SDatabase = ShoeStore.Data.Database;

static void Check(bool condition, string label) { if (!condition) throw new Exception(label); Console.WriteLine("PASS: " + label); }
static async Task Rejected(Func<Task> action, string label)
{
    try { await action(); }
    catch (Exception e) when (e is ArgumentException or UnauthorizedAccessException) { Console.WriteLine("PASS: " + label); return; }
    throw new Exception("Expected rejection: " + label);
}
static Puzzle Solved()
{
    var puzzle = new Puzzle();
    for (var i = 0; i < 4; i++) puzzle.Swap(i, Enumerable.Range(0, 4).Single(j => puzzle.Tiles[j] == i + 1));
    return puzzle;
}
var boundary = DiscountCalculator.Period(new DateTime(2027, 1, 31));
Check(boundary == (new DateTime(2026, 12, 1), new DateTime(2027, 1, 1)), "previous calendar month crosses year boundary");
Check(DiscountCalculator.Price(4000m, false) == 3000m && DiscountCalculator.Price(4000m, true) == 4000m, "25% discount depends on model history");
Check(DiscountCalculator.Price(1.02m, false) == .77m, "money rounds away from zero to kopecks");
for (var i = 0; i < 100; i++) { var puzzle = new Puzzle(); if (puzzle.IsSolved || !puzzle.Tiles.Order().SequenceEqual(new[] { 1,2,3,4 })) throw new Exception("Puzzle shuffle invariant"); }
Console.WriteLine("PASS: 100 shuffled puzzles keep all fragments and start unsolved");
Check(Solved().IsSolved, "fragment swaps solve the actual puzzle");

// Integration checks create fresh databases. Only run against a disposable local PostgreSQL.
var adminConnection = Environment.GetEnvironmentVariable("WPF_TEST_POSTGRES");
if (adminConnection is null) { Console.WriteLine("PostgreSQL integration skipped: set WPF_TEST_POSTGRES to a disposable cluster connection."); return; }
var root = Path.GetFullPath(args.Length > 0 ? args[0] : ".");
var suffix = Guid.NewGuid().ToString("N")[..10];
var examName = "wpf_exam_" + suffix; var shoeName = "wpf_shoe_" + suffix;
await using var admin = new NpgsqlConnection(adminConnection); await admin.OpenAsync();
async Task AdminSql(string sql) { await using var cmd = new NpgsqlCommand(sql, admin); await cmd.ExecuteNonQueryAsync(); }
string Connection(string name) { var b = new NpgsqlConnectionStringBuilder(adminConnection) { Database = name }; return b.ToString(); }
IConfiguration Config(string name) => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> { ["Database:Provider"] = "Postgres", ["ConnectionStrings:Postgres"] = Connection(name), ["CalculationDate"] = "2026-05-15" }).Build();
async Task Sql(string db, string text) { await using var c = new NpgsqlConnection(Connection(db)); await c.OpenAsync(); await using var cmd = new NpgsqlCommand(text, c); await cmd.ExecuteNonQueryAsync(); }
async Task<int> Number(string db, string sql) { await using var c = new NpgsqlConnection(Connection(db)); await c.OpenAsync(); await using var cmd = new NpgsqlCommand(sql, c); return Convert.ToInt32(await cmd.ExecuteScalarAsync()); }
await AdminSql($"CREATE DATABASE {examName}"); await AdminSql($"CREATE DATABASE {shoeName}");
try
{
    await Sql(examName, await File.ReadAllTextAsync(Path.Combine(root,"examples/ExamGuide/sql/postgres-schema.sql")));
    await Sql(examName, await File.ReadAllTextAsync(Path.Combine(root,"examples/ExamGuide/sql/postgres-seed.sql")));
    var users = new UserRepository(new EDatabase(Config(examName)));
    await users.SeedAsync(); await users.SeedAsync();
    await users.ImportCustomersAsync(Path.Combine(root,"examples/ExamGuide/Data/customers.json"));
    Check(await Number(examName,"SELECT COUNT(*) FROM customers WHERE id<>'DEMO00001'") == 6, "WPF preparation imports six customers");
    Check((await new NotesRepository(new EDatabase(Config(examName))).GetAsync(null)).Count == 3, "shared API repository returns three notes without duplicate seed");
    var auth = new AuthService(users);
    await Rejected(() => auth.LoginAsync("user", "", Solved()), "empty password is rejected");
    Check((await users.FindAsync("user"))!.FailedAttempts == 0, "empty fields do not count as login failure");
    await Rejected(() => auth.LoginAsync("user", "wrong", Solved()), "wrong password counts one failure");
    await Rejected(() => auth.LoginAsync("user", "User123!", new Puzzle()), "wrong puzzle counts one failure");
    await Rejected(() => auth.LoginAsync("user", "wrong", Solved()), "third login failure blocks user");
    Check((await users.FindAsync("user"))!.IsBlocked, "blocked account persists in database");
    await Rejected(() => new AuthService(users).LoginAsync("user", "User123!", Solved()), "new WPF session cannot bypass block");
    var user = (await users.FindAsync("user"))!;
    await users.EditAsync(user.Id,user.Login,null,user.Role,true);
    await auth.LoginAsync("user", "User123!", Solved());
    await Rejected(async () => { await auth.RequireUserAsync(true); }, "ordinary WPF user cannot manage users");
    await auth.LoginAsync("admin", "Admin123!", Solved());
    var adminUser = await auth.RequireUserAsync(true);
    await users.EditAsync(adminUser.Id,adminUser.Login,null,"Пользователь",false);
    await Rejected(async () => { await auth.RequireUserAsync(true); }, "role changes revoke access of already open admin window");

    await Sql(shoeName, await File.ReadAllTextAsync(Path.Combine(root,"examples/ShoeStore/sql/postgres-schema.sql")));
    var db = new SDatabase(Config(shoeName)); var importer = new ImportService(db,Path.Combine(root,"examples/ShoeStore/Data"));
    await importer.RunAsync();
    Check(await Number(shoeName,"SELECT COUNT(*) FROM products") == 31 && await Number(shoeName,"SELECT COUNT(*) FROM stock_items") == 92, "WPF imports 31 models and 92 positions");
    Check(await Number(shoeName,"SELECT COUNT(*) FROM sizes") == 35 && await Number(shoeName,"SELECT COUNT(*) FROM order_items") == 30, "fractional sizes and historical order lines survive import");
    var catalog = new CatalogRepository(db); var repository = new OrderRepository(db); var clock = new StoreClock(Config(shoeName));
    var store = new StoreService(catalog,repository,clock);
    await Rejected(async () => { await store.OrdersAsync(); }, "guest cannot access order window data");
    await store.LoginAsync("asidorova");
    await Rejected(async () => { await store.OrdersAsync(); }, "User cannot access orders");
    var products = await store.ProductsAsync();
    var choices = new List<(Product Product,StockItem Stock)>();
    foreach (var product in products) { var sku = (await store.StockAsync(product.Id)).FirstOrDefault(s => s.Available > 5); if (sku is not null) choices.Add((product,sku)); if (choices.Count == 2) break; }
    var first = choices[0]; var second = choices[1];
    await store.AddAsync(first.Product.Id,first.Stock.Id,2); await store.AddAsync(second.Product.Id,second.Stock.Id,1);
    var before = await Number(shoeName,$"SELECT available_quantity FROM stock_items WHERE id={first.Stock.Id}");
    var orderId = await store.ConfirmAsync((await catalog.FindUserAsync("isivanov"))!.Id);
    Check(await Number(shoeName,$"SELECT user_id FROM orders WHERE id={orderId}") == store.Current!.Id, "User cannot assign another client even through direct service call");
    Check((await repository.LinesAsync(orderId)).Count == 2 && store.BasketCount == 0, "two-line order commits and clears WPF basket");
    Check(await Number(shoeName,$"SELECT available_quantity FROM stock_items WHERE id={first.Stock.Id}") == before - 2, "confirmation deducts stock");
    await store.LoginAsync("papetrov");
    await Rejected(() => store.ChangeDateAsync(orderId,clock.Today), "Manager cannot change order date");
    await Rejected(async () => await store.RemoveLineAsync(orderId,(await repository.LinesAsync(orderId))[0].Id), "Manager cannot remove individual line");
    await store.RemoveOrderAsync(orderId);
    Check(await Number(shoeName,$"SELECT available_quantity FROM stock_items WHERE id={first.Stock.Id}") == before, "Manager deletion restores all stock");
    await store.AddAsync(first.Product.Id,first.Stock.Id,1); store.Cancel();
    Check(await Number(shoeName,$"SELECT available_quantity FROM stock_items WHERE id={first.Stock.Id}") == before, "cancelled draft never changes database stock");
    var count = await Number(shoeName,"SELECT COUNT(*) FROM orders");
    await Rejected(() => repository.CreateAsync(store.Current!.Id,[new(first.Stock.Id,1),new(second.Stock.Id,int.MaxValue)],clock.Today), "insufficient second line rejects entire transaction");
    Check(await Number(shoeName,$"SELECT available_quantity FROM stock_items WHERE id={first.Stock.Id}") == before && await Number(shoeName,"SELECT COUNT(*) FROM orders") == count, "failed second line rolls back first line and order header");
    await store.LoginAsync("isivanov");
    await store.AddAsync(first.Product.Id,first.Stock.Id,1); await store.AddAsync(second.Product.Id,second.Stock.Id,1);
    var id = await store.ConfirmAsync(null); var lines = await store.LinesAsync(id); var originalPrice = lines[1].UnitPrice;
    await store.ChangeDateAsync(id,new DateTime(2026,4,1));
    Check((await store.LinesAsync(id))[1].UnitPrice == originalPrice, "Admin date edit preserves historical price");
    await store.RemoveLineAsync(id,lines[0].Id);
    Check((await store.LinesAsync(id)).Count == 1, "Admin removes one line and keeps other line");
    await store.RemoveOrderAsync(id);
}
finally
{
    NpgsqlConnection.ClearAllPools();
    await AdminSql($"DROP DATABASE {examName} WITH (FORCE)"); await AdminSql($"DROP DATABASE {shoeName} WITH (FORCE)");
}
Console.WriteLine("All PostgreSQL integration checks passed.");
