using System.Security.Claims;
using System.Text.Json;
using ExamGuide.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExamGuide.Pages;
public class LoginModel(UserRepository users) : PageModel
{
    [BindProperty] public string Login { get; set; } = "";
    [BindProperty] public string Password { get; set; } = "";
    [BindProperty] public string Puzzle { get; set; } = "";
    public int[] Tiles { get; set; } = [];
    public string Message { get; set; } = "";

    private void NewPuzzle()
    {
        // Правильный порядок IDs хранится на сервере, не берётся из hidden-поля клиента.
        var ids = Enumerable.Range(1,4).OrderBy(_ => Random.Shared.Next()).ToArray();
        HttpContext.Session.SetString("PuzzleIds",JsonSerializer.Serialize(ids));
        Tiles = Enumerable.Range(0,4).OrderBy(_ => Random.Shared.Next()).ToArray();
        if (Tiles.SequenceEqual(ids.Select((id,index) => (id,index)).OrderBy(x=>x.id).Select(x=>x.index)))
            (Tiles[0],Tiles[1]) = (Tiles[1],Tiles[0]);
    }
    public void OnGet() => NewPuzzle();
    public async Task<IActionResult> OnPostAsync()
    {
        var idsJson = HttpContext.Session.GetString("PuzzleIds");
        HttpContext.Session.Remove("PuzzleIds"); // Каждый выданный пазл используется один раз.
        if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
            return Fail("Заполните обязательные поля: логин и пароль");
        var user = await users.FindAsync(Login);
        if (user?.IsBlocked == true) return Fail("Вы заблокированы. Обратитесь к администратору");
        var validPuzzle = false;
        try
        {
            var ids = idsJson is null ? null : JsonSerializer.Deserialize<int[]>(idsJson);
            var order = JsonSerializer.Deserialize<int[]>(Puzzle);
            validPuzzle = ids?.Length == 4 && order?.Length == 4 && order.Distinct().Count()==4
                && order.All(index => index >= 0 && index < 4)
                && order.Select(index => ids[index]).SequenceEqual(new[] { 1,2,3,4 });
        }
        catch (JsonException) { }
        if (user is null || !validPuzzle || !users.Verify(user,Password))
        {
            if (user is not null)
            {
                // Одна попытка формы = одна ошибка, даже если неверны и пароль, и пазл.
                await users.RegisterFailureAsync(user.Id);
                if ((await users.FindAsync(Login))?.IsBlocked == true)
                    return Fail("Вы заблокированы. Обратитесь к администратору");
            }
            return Fail(!validPuzzle ? "Пазл собран неверно. Расставьте фрагменты и повторите вход" :
                "Вы ввели неверный логин или пароль. Пожалуйста проверьте ещё раз введенные данные");
        }
        await users.ResetFailuresAsync(user.Id);
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
            new Claim(ClaimTypes.Name,user.Login),new Claim(ClaimTypes.Role,user.Role) },CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(identity));
        TempData["Message"] = "Вы успешно авторизовались";
        return RedirectToPage(user.Role=="Администратор" ? "/Admin/Index" : "/Index");
    }
    private IActionResult Fail(string message) { Message=message; Password=""; ModelState.Remove(nameof(Password)); NewPuzzle(); return Page(); }
}
