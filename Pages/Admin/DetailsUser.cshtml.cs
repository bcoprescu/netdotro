#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace netdotro.Admin.Details
{
    public class DetailsUserModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ApplicationUser CurrentUser {get; private set; }
         public DetailsUserModel(UserManager<ApplicationUser> userManager)
         {
            _userManager = userManager;
         }
         public async Task<IActionResult> OnGetAsync([Required] string id) {
            if (id != null) {
                CurrentUser = await _userManager.FindByIdAsync(id);
            }
            if (CurrentUser == null)
            {
                return NotFound($"Unable to load user with ID '{id}'.");
            }
            return Page();
        }
    }
}
