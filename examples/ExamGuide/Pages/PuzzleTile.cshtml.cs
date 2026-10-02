using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace ExamGuide.Pages;
public class PuzzleTileModel(IWebHostEnvironment environment) : PageModel
{
    public IActionResult OnGet(int index)
    {
        var json = HttpContext.Session.GetString("PuzzleIds");
        if (json is null || index < 0 || index > 3) return NotFound();
        var ids = JsonSerializer.Deserialize<int[]>(json)!;
        return PhysicalFile(Path.Combine(environment.WebRootPath,"puzzle",$"{ids[index]}.png"),"image/png");
    }
}
