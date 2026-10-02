using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeStore.Data;
namespace ShoeStore.Pages;
public class LoginModel(CatalogRepository repository) : PageModel
{
    [BindProperty] public string Login { get; set; }="";
    public string Error { get; set; }="";
    public void OnGet() { }
    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Login))
        {
            Error="Укажите логин из списка пользователей";
            return Page();
        }
        var user=await repository.FindUserAsync(Login);
        if (user is null)
        {
            Error="Логин не найден. Проверьте написание и повторите вход";
            return Page();
        }
        var identity=new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
            new Claim(ClaimTypes.Name,user.Login),
            new Claim(ClaimTypes.Role,user.Role),
            new Claim("FullName",user.FullName)
        },CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(identity));
        TempData["Message"]="Вы вошли в систему";
        return RedirectToPage("/Index");
    }
    public async Task<IActionResult> OnPostLogoutAsync()
    {
        await HttpContext.SignOutAsync();
        HttpContext.Session.Clear();
        return RedirectToPage("/Index");
    }
}
