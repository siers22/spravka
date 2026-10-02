using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShoeStore.Data;
namespace ShoeStore.Pages;
public class IndexModel(CatalogRepository repository,StoreClock clock) : PageModel
{
    public List<Product> Products { get; set; }=[];
    public List<string> Categories { get; set; }=[];
    [BindProperty(SupportsGet=true)] public string Query { get; set; }="";
    [BindProperty(SupportsGet=true)] public string Category { get; set; }="";
    [BindProperty(SupportsGet=true)] public string Sort { get; set; }="asc";
    private async Task LoadAsync()
    {
        var all=await repository.ProductsAsync(clock.Today);
        Categories=all.Select(product=>product.Category).Distinct().Order().ToList();
        IEnumerable<Product> selected=all;
        if (User.Identity?.IsAuthenticated==true)
        {
            if (!string.IsNullOrWhiteSpace(Category)) selected=selected.Where(product=>product.Category==Category);
            if (!string.IsNullOrWhiteSpace(Query)) selected=selected.Where(product=>
                product.Name.Contains(Query,StringComparison.OrdinalIgnoreCase)||product.Description.Contains(Query,StringComparison.OrdinalIgnoreCase));
            selected=Sort=="desc" ? selected.OrderByDescending(product=>product.FinalPrice).ThenBy(product=>product.Id)
                : selected.OrderBy(product=>product.FinalPrice).ThenBy(product=>product.Id);
        }
        Products=selected.ToList();
    }
    public async Task OnGetAsync() => await LoadAsync();
    public async Task<IActionResult> OnGetListAsync()
    {
        await LoadAsync();
        return Partial("_Products",Products);
    }
}
