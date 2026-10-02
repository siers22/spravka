using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace ExamGuide.Pages;
public class IndexModel : PageModel
{
    public void OnGet() { }
    public async Task<IActionResult> OnPostLogoutAsync() { await HttpContext.SignOutAsync(); HttpContext.Session.Clear(); return RedirectToPage("/Login"); }
}
