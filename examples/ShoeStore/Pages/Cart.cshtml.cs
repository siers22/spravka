using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeStore.Data;
namespace ShoeStore.Pages;
public class CartModel(CatalogRepository catalog,OrderRepository orders,StoreClock clock) : PageModel
{
    public List<(BasketLine Line,Product Product,StockItem Stock)> Rows { get; set; }=[];
    public List<StoreUser> Users { get; set; }=[];
    public string Error { get; set; }="";
    public bool Staff => User.IsInRole("Admin")||User.IsInRole("Manager");
    public async Task<IActionResult> OnGetAsync()
    {
        if (User.Identity?.IsAuthenticated!=true) return Challenge();
        var products=await catalog.ProductsAsync(clock.Today);
        foreach (var line in BasketSession.Read(HttpContext.Session))
        foreach (var product in products)
        {
            var stock=(await catalog.StockAsync(product.Id)).FirstOrDefault(item=>item.Id==line.StockItemId);
            if (stock is not null) Rows.Add((line,product,stock));
        }
        if (Staff) Users=await catalog.UsersAsync();
        return Page();
    }
    public IActionResult OnPostCancel()
    {
        if (User.Identity?.IsAuthenticated!=true) return Challenge();
        HttpContext.Session.Remove("Basket");
        TempData["Message"]="Вы отказались от неподтверждённого заказа. Остатки не изменены";
        return RedirectToPage("/Index");
    }
    public async Task<IActionResult> OnPostQuantityAsync(int stockItemId,int quantity)
    {
        if (User.Identity?.IsAuthenticated!=true) return Challenge();
        await OnGetAsync();
        var row=Rows.FirstOrDefault(row=>row.Stock.Id==stockItemId);
        if (!ModelState.IsValid || row.Stock is null || quantity<0 || quantity>row.Stock.Available)
        {
            Error="Количество должно быть целым числом от 0 до доступного остатка. Ноль удаляет позицию";
            return Page();
        }
        var basket=BasketSession.Read(HttpContext.Session);
        basket.RemoveAll(line=>line.StockItemId==stockItemId);
        if (quantity>0) basket.Add(new BasketLine(stockItemId,quantity));
        BasketSession.Write(HttpContext.Session,basket);
        return RedirectToPage();
    }
    public async Task<IActionResult> OnPostConfirmAsync(int? customerId)
    {
        if (User.Identity?.IsAuthenticated!=true) return Challenge();
        var userId=int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (Staff && customerId.HasValue) userId=customerId.Value;
        if (!(await catalog.UsersAsync()).Any(user=>user.Id==userId))
        {
            Error="Выберите существующего клиента";
            await OnGetAsync();
            return Page();
        }
        try
        {
            var id=await orders.CreateAsync(userId,BasketSession.Read(HttpContext.Session),clock.Today);
            HttpContext.Session.Remove("Basket");
            TempData["Message"]=$"Заказ № {id} оформлен. Остатки обновлены";
            return RedirectToPage("/Index");
        }
        catch (ArgumentException exception) { Error=exception.Message; }
        catch (Exception) { Error="Не удалось сохранить заказ. Обновите каталог и повторите попытку. Частичные изменения отменены"; }
        await OnGetAsync();
        return Page();
    }
}
