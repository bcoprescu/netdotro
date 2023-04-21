#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MyApp.Namespace
{
    [Authorize(Roles = "Admin")]
    public class RolesModel : PageModel
    {
        private readonly ILogger<RolesModel> _logger;
        private RoleManager<IdentityRole> _roleManager;
        public List<IdentityRole> roles { get; set; } = new List<IdentityRole>();
        [TempData]
        public string StatusMessage { get; set; }

        public RolesModel(ILogger<RolesModel> logger, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            roles = await _roleManager.Roles.ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync([Required] string name)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = await _roleManager.CreateAsync(new IdentityRole(name));
                if (result.Succeeded){
                    StatusMessage = $"New role {name} added";
                }
                else{
                    StatusMessage = $"Error: Failed to add new role";
                }
            }
            return RedirectToPage();
        }

          public async Task<IActionResult> OnPostDeleteAsync([Required] string id)
        {
            if (ModelState.IsValid)
            {
                IdentityRole role = await _roleManager.FindByIdAsync(id);
                if (role == null) {
                    StatusMessage = $"Error: Failed to delete role with id {id}";
                    return RedirectToPage();
                }
                IdentityResult result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded){
                    StatusMessage = $"Removed role for id {id}";
                }
                else{
                    StatusMessage = $"Error: Failed to delete role with {id}";
                }
            }
            return RedirectToPage();
        }
    }
}
