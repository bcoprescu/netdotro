using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace netdotro.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
 
    public IndexModel(ILogger<IndexModel> logger, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;

    }
  
    public void OnGet()
    {

    }
}
