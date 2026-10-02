using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeStore.Data;
namespace ShoeStore.Pages;
public class ProductModel(CatalogRepository repository,StoreClock clock) : PageModel
{
    public Product? Product { get; set; }
    public List<StockItem> Stock { get; set; }=[];
    public string Error { get; set; }="";
    [BindProperty] public int StockItemId { get; set; }
    [BindProperty] public int Quantity { get; set; }=1;
    public async Task<IActionResult> OnGetAsync(int id)
    {
        Product=(await repository.ProductsAsync(clock.Today)).FirstOrDefault(product=>product.Id==id);
        if (Product is null) return NotFound();
        Stock=await repository.StockAsync(id);
        return Page();
    }
    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (User.Identity?.IsAuthenticated!=true) return Challenge();
        var loaded=await OnGetAsync(id);
        if (Product is null) return loaded;
        var item=Stock.FirstOrDefault(stock=>stock.Id==StockItemId);
        var basket=BasketSession.Read(HttpContext.Session);
        var previous=basket.FirstOrDefault(line=>line.StockItemId==StockItemId)?.Quantity??0;
        if (!ModelState.IsValid || item is null || Quantity<1 || (long)Quantity+previous>item.Available)
        {
            Error="Выберите доступный размер и целое количество от 1 до доступного остатка с учётом корзины";
            return Page();
        }
        basket.RemoveAll(line=>line.StockItemId==StockItemId);
        basket.Add(new BasketLine(StockItemId,Quantity+previous));
        BasketSession.Write(HttpContext.Session,basket);
        TempData["Message"]="Товар добавлен. Продолжите выбор или подтвердите заказ в корзине";
        return RedirectToPage("/Cart");
    }
}
