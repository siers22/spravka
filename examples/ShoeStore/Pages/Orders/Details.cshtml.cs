using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeStore.Data;
namespace ShoeStore.Pages.Orders;
public class DetailsModel(OrderRepository repository) : PageModel
{
    public OrderHeader? Order { get; set; }
    public List<OrderLine> Lines { get; set; }=[];
    public string Error { get; set; }="";
    public async Task<IActionResult> OnGetAsync(int id)
    {
        Order=(await repository.AllAsync()).FirstOrDefault(order=>order.Id==id);
        if (Order is null) return NotFound();
        Lines=await repository.LinesAsync(id);
        return Page();
    }
    public async Task<IActionResult> OnPostDateAsync(int id,DateTime date)
    {
        if (!User.IsInRole("Admin")) return Forbid();
        if (!ModelState.IsValid || date==default)
        {
            await OnGetAsync(id);
            Error="Введите корректную дату";
            return Page();
        }
        try
        {
            await repository.ChangeDateAsync(id,date);
            return RedirectToPage(new { id });
        }
        catch (Exception) { Error="Не удалось изменить дату. Проверьте данные и повторите действие"; }
        return await OnGetAsync(id);
    }
    public async Task<IActionResult> OnPostRemoveAsync(int id,int itemId)
    {
        if (!User.IsInRole("Admin")) return Forbid();
        try
        {
            await repository.RemoveAsync(id,itemId);
            TempData["Message"]="Позиция удалена, остаток восстановлен. Пустой заказ удаляется целиком";
            return RedirectToPage("/Orders/Index");
        }
        catch (Exception) { Error="Не удалось удалить позицию. Обновите список и повторите действие"; }
        return await OnGetAsync(id);
    }
}
