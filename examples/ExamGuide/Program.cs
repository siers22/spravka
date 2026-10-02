using ExamGuide.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages(options => options.Conventions.AuthorizeFolder("/Admin", "Admin"));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
     .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/Denied";
        options.Events.OnValidatePrincipal = async context =>
        {
            // Изменение роли, логина или блокировки действует и на уже выданную cookie.
            var repository = context.HttpContext.RequestServices.GetRequiredService<UserRepository>();
            var user = await repository.FindAsync(context.Principal?.Identity?.Name ?? "");
            if (user is null || user.IsBlocked) { context.RejectPrincipal(); return; }
            var oldRole = context.Principal?.FindFirstValue(ClaimTypes.Role);
            if (oldRole != user.Role)
            {
                var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                    new Claim(ClaimTypes.Name,user.Login),new Claim(ClaimTypes.Role,user.Role) },CookieAuthenticationDefaults.AuthenticationScheme);
                context.ReplacePrincipal(new ClaimsPrincipal(identity));
                context.ShouldRenew = true;
            }
        };
    });
builder.Services.AddAuthorization(options => options.AddPolicy("Admin", policy => policy.RequireRole("Администратор")));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => { options.Cookie.HttpOnly = true; options.Cookie.IsEssential = true; options.IdleTimeout = TimeSpan.FromMinutes(15); });
builder.Services.AddSingleton<Database>();
builder.Services.AddScoped<NotesRepository>();
builder.Services.AddScoped<UserRepository>();
var app = builder.Build();
app.UseExceptionHandler(handler => handler.Run(async context =>
{
    context.Response.StatusCode = 500;
    if (context.Request.Path.StartsWithSegments("/notes"))
        await context.Response.WriteAsJsonAsync(new { error = "Внутренняя ошибка сервера" });
    else
    {
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.WriteAsync("Ошибка сервера. Проверьте подключение к БД и повторите действие. <a href='/'>На главную</a>");
    }
}));
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Строковый параметр разбираем сами: некорректное значение должно дать именно 400.
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
app.MapRazorPages();

// Явная команда подготовки, а не скрытый импорт при каждом старте.
if (args.Contains("--seed"))
{
    using var scope = app.Services.CreateScope();
    var users = scope.ServiceProvider.GetRequiredService<UserRepository>();
    await users.SeedAsync();
    await users.ImportCustomersAsync(Path.Combine(app.Environment.ContentRootPath, "Data", "customers.json"));
    Console.WriteLine("Добавлены admin/Admin123! и user/User123!, импортированы 6 контрагентов.");
    return;
}
app.Run();
public partial class Program { }
