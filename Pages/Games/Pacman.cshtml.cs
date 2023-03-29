using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace netdotro.Games;

public class PacmanModel : PageModel
{
    private readonly ILogger<PacmanModel> _logger;

    public PacmanModel(ILogger<PacmanModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
    }
}

