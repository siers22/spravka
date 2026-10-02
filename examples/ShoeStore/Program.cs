using ShoeStore.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
var builder=WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages(options=>options.Conventions.AuthorizeFolder("/Orders","Staff"));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options=>
{
    options.LoginPath="/Login";
    options.AccessDeniedPath="/Denied";
});
builder.Services.AddAuthorization(options=>options.AddPolicy("Staff",policy=>policy.RequireRole("Admin","Manager")));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options=>
{
    options.Cookie.IsEssential=true;
    options.Cookie.HttpOnly=true;
});
builder.Services.AddSingleton<Database>();
builder.Services.AddSingleton<StoreClock>();
builder.Services.AddScoped<CatalogRepository>();
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<ImportService>();
var app=builder.Build();
app.UseExceptionHandler(handler=>handler.Run(async context=>
{
    context.Response.StatusCode=500;
    context.Response.ContentType="text/html; charset=utf-8";
    await context.Response.WriteAsync("⚠ Ошибка. Проверьте подключение к базе данных и повторите действие. <a href='/'>В каталог</a>");
}));
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
if (args.Contains("--import"))
{
    using var scope=app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<ImportService>().RunAsync();
    return;
}
app.Run();
