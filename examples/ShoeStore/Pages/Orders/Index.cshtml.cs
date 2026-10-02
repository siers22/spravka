using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeStore.Data;
namespace ShoeStore.Pages.Orders;
public class IndexModel(OrderRepository repository) : PageModel
{
    public List<OrderHeader> Orders { get; set; }=[];
    public string Error { get; set; }="";
    public async Task OnGetAsync() => Orders=await repository.AllAsync();
    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            await repository.RemoveAsync(id);
            TempData["Message"]="Заказ удалён, пары возвращены в доступный остаток";
            return RedirectToPage();
        }
        catch (ArgumentException exception) { Error=exception.Message; }
        catch (Exception) { Error="Не удалось удалить заказ. Проверьте БД и повторите действие"; }
        await OnGetAsync();
        return Page();
    }
}
