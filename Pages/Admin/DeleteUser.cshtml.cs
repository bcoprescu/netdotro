#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace netdotro.Admin.DeleteUser
{
    [Authorize(Roles = "Admin")]
    public class DeleteUserModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public string? UserName { get; set; }
        public readonly IEmailSender _emailSender;
        private readonly ILogger<DeleteUserModel> _logger;
        public byte[] ProfilePicture { get; set; }
        [BindProperty]
        public InputModel Input { get; set; }
        public string UserId;
        [TempData]
        public string StatusMessage { get; set; }
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }
    
        public DeleteUserModel(UserManager<ApplicationUser> userManager,
            IEmailSender emailSender, 
            ILogger<DeleteUserModel> logger)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }
        public async Task<IActionResult> OnGetAsync(string id)
        {
            ApplicationUser user = null;
            if (id != null) {
                user = await _userManager.FindByIdAsync(id);
            }
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{id}'.");
            }

            UserName = user.UserName;
            ProfilePicture = user.ProfilePicture;
            UserId = user.Id;
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(string id)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{id}'.");
            }
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            if (currentUser.Id == user.Id) {
                StatusMessage = "Error: Cannot remove your own account from this page!";
                return Page();
            }
            if (!await _userManager.CheckPasswordAsync(currentUser, Input.Password))
            {
                ModelState.AddModelError(string.Empty, "Incorrect password.");
                return Page();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleting user.");
            }

            await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                $"Your BCO account has been deleted");

            _logger.LogInformation("User with ID '{UserId}' was deleted by {curentUser}.", id, currentUser.UserName);

            StatusMessage = $"Permanently deleted {user.UserName}";
            return RedirectToPage("/Admin/Users");
        }
    }
}
