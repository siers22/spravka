using ExamGuide.Data;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<Database>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<NotesRepository>();
var app = builder.Build();
app.MapGet("/notes", async (HttpContext context, NotesRepository repository, ILogger<Program> logger) =>
{
    int? userId = null;
    foreach (var key in context.Request.Query.Keys)
        if (key != "user_id") return Results.BadRequest(new { error = "Неизвестный параметр запроса" });
    if (context.Request.Query.TryGetValue("user_id", out var value))
    {
        if (value.Count != 1 || !int.TryParse(value[0], out var parsed) || parsed < 1)
            return Results.BadRequest(new { error = "user_id должен быть положительным целым числом" });
        userId = parsed;
    }
    try { return Results.Ok(await repository.GetAsync(userId)); }
    catch (Exception exception)
    {
        logger.LogError(exception, "Ошибка чтения заметок");
        return Results.Json(new { error = "Ошибка подключения к базе данных или чтения заметок" }, statusCode: 500);
    }
});
if (args.Contains("--seed"))
{
    using var scope = app.Services.CreateScope();
    var users = scope.ServiceProvider.GetRequiredService<UserRepository>();
    await users.SeedAsync();
    await users.ImportCustomersAsync(Path.Combine(AppContext.BaseDirectory, "Data", "customers.json"));
    Console.WriteLine("Подготовлены admin, user, 3 заметки и 6 контрагентов.");
    return;
}
app.Run();
public partial class Program { }
