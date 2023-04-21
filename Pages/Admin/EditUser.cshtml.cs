#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace netdotro.Admin.EditUser
{
    [Authorize(Roles = "Admin")]
    public class EditUserModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        public EditUserModel(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender, 
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _roleManager = roleManager;
        }
        public string Email { get; set; }
        public bool IsEmailConfirmed { get; set; }
        [BindProperty]
        public string CurrentID { get; set; }
        public string Username { get; set; }
        [TempData]
        public string StatusMessage { get; set; }
        [BindProperty]
        public InputModel Input { get; set; }
        public class InputModel
        {

            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Profile picture")]
            public byte[] ProfilePicture { get; set; }
            
            [Required]
            [EmailAddress]
            [Display(Name = "New email")]
            public string NewEmail { get; set; }
            [Display(Name = "Roles")]
            public Dictionary<string, bool> Roles { get; set; } = new Dictionary<string, bool>();
        }
        
        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            
            Email = await _userManager.GetEmailAsync(user);

            Username = userName;
            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
            Input = new InputModel
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = phoneNumber,
                    ProfilePicture = user.ProfilePicture,
                    NewEmail = Email,
                };
            
            var allRoles = await _roleManager.Roles.ToListAsync();
            foreach (IdentityRole role in allRoles){
                if (await _userManager.IsInRoleAsync(user, role.Name)) {
                    Input.Roles[role.Name] = true;
                }
                else {
                    Input.Roles[role.Name] = false;
                }
            }
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            CurrentID = id;
            ApplicationUser user = null;
            if (id != null) {
                user = await _userManager.FindByIdAsync(id);
            }
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{id}'.");
            }

            await LoadAsync(user);
            return Page();
        }

    
        public async Task<IActionResult> OnPostUpdateDataAsync(string id, string[] roles)
        {
            ApplicationUser user = null;
            if (id != null) {
                user = await _userManager.FindByIdAsync(id);
            }
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{id}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage("/Admin/EditUser", new {id = id});
                }
            }

            if (user.FirstName != Input.FirstName || user.LastName != Input.LastName) {
                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;
                var nameResult = await _userManager.UpdateAsync(user);
                if (!nameResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set name.";
                    return RedirectToPage("/Admin/EditUser", new {id = id});
                }
            }
            string email = await _userManager.GetEmailAsync(user);
            if (Input.NewEmail != email)
            {
                var code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmailChange",
                    pageHandler: null,
                    values: new { area = "Identity", userId = id, email = Input.NewEmail, code = code },
                    protocol: Request.Scheme);
                await _emailSender.SendEmailAsync(
                    Input.NewEmail,
                    "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                StatusMessage = "Confirmation link to change email sent. Please check your email.";
               return RedirectToPage("/Admin/EditUser", new {id = id});
            }

            await UpdateRolesAsync(user, roles);
            StatusMessage = "User profile has been updated";
            return RedirectToPage("/Admin/EditUser", new {id = id});
        }

        private async Task UpdateRolesAsync(ApplicationUser user, string[] roles)
        {
            var allRoles = await _roleManager.Roles.ToListAsync();
            foreach(var role in allRoles) {
                if (roles.Contains(role.Name)) {
                    await _userManager.AddToRoleAsync(user, role.Name);
                }
                else {
                    await _userManager.RemoveFromRoleAsync(user, role.Name);
                }
            }
        }
    }
}
