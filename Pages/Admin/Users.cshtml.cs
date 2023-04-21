#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace netdotro.UsersModel
{
    [Authorize(Roles = "Admin")]
    public class UsersModel : PageModel
    {
        public const int ORDER_NONE = 0;
        public const int ORDER_NAME_ASC = 1;
        public const int ORDER_NAME_DSC = 2;
        public const int ORDER_MAIL_ASC = 3;
        public const int ORDER_MAIL_DSC = 4;

        private readonly ILogger<UsersModel> _logger;
        private UserManager<ApplicationUser> _userManager;
        public List<ApplicationUser> users { get; set; } = new List<ApplicationUser>();
        public string SearchFilter { get; private set; }
        [TempData]
        public string StatusMessage { get; set; }
        public int OrderBy = ORDER_NONE;
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; private set; }

        public UsersModel(ILogger<UsersModel> logger, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }
        public async Task<IActionResult> OnGetAsync(int orderBy, string searchString, int pageIndex)
        {
            users = await _userManager.Users.ToListAsync();
            SearchFilter = searchString;
            OrderBy = orderBy != ORDER_NONE ? orderBy : OrderBy;
            switch (orderBy) {
                case ORDER_MAIL_ASC:
                    users = users.OrderBy(s => s.UserName).ToList();
                    break;
                case ORDER_MAIL_DSC:
                    users = users.OrderByDescending(s => s.UserName).ToList();
                    break;
                case ORDER_NAME_ASC:
                    users = users.OrderBy(s => s.FirstName).ToList();
                    break;
                case ORDER_NAME_DSC:
                    users = users.OrderByDescending(s => s.FirstName).ToList();
                break;
                case ORDER_NONE:
                    default:
                    break;

            }
            if (!String.IsNullOrEmpty(searchString)) {
                users = users.Where(s => s.UserName.Contains(searchString)).ToList();
                PageIndex = 1;
            }
            PageIndex = pageIndex > 0 && pageIndex < users.Count ? pageIndex : 1;
            PageSize = 5;
            users = PaginatedList(users, PageIndex, PageSize);
            return Page();
        }
        public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return roles.ToList();
        }

        private List<ApplicationUser> PaginatedList(List<ApplicationUser> items, int pageIndex, int pageSize)  
        {  
            List<ApplicationUser> ret = new List<ApplicationUser>();
            var count = items.Count;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            ret = items.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            return ret;
        }
        public bool HasPreviousPage  
        {  
            get   { return (PageIndex > 1); }  
        }
        public bool HasNextPage  
        {  
            get { return (PageIndex < TotalPages); }  
        } 
    }
}
