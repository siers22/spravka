using System.Security.Claims;
using ExamGuide.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace ExamGuide.Pages.Admin;
public class IndexModel(UserRepository repository, ILogger<IndexModel> logger) : PageModel
{
    public List<AppUser> Users { get; set; } = [];
    public string Message { get; set; } = "";
    public async Task OnGetAsync() => Users=await repository.AllAsync();
    public async Task<IActionResult> OnPostAddAsync(string login,string password,string role)
    {
        try { await repository.AddAsync(login ?? "",password ?? "",role); TempData["Message"]="Пользователь добавлен"; return RedirectToPage(); }
        catch (ArgumentException ex) { Message=ex.Message; }
        catch (Exception ex) { logger.LogError(ex,"Добавление пользователя"); Message="Не удалось сохранить. Проверьте БД, уникальность логина и длину полей"; }
        await OnGetAsync(); return Page();
    }
    public async Task<IActionResult> OnPostEditAsync(int id,string login,string? password,string role,bool unblock)
    {
        try
        {
            if (User.FindFirstValue(ClaimTypes.NameIdentifier)==id.ToString() && role!="Администратор")
                throw new ArgumentException("Нельзя снять роль администратора у собственной активной учётной записи");
            await repository.EditAsync(id,login ?? "",password,role,unblock);
            TempData["Message"]="Данные пользователя сохранены"; return RedirectToPage();
        }
        catch (ArgumentException ex) { Message=ex.Message; }
        catch (Exception ex) { logger.LogError(ex,"Изменение пользователя"); Message="Не удалось сохранить. Проверьте БД, уникальность логина и длину полей"; }
        await OnGetAsync(); return Page();
    }
}
