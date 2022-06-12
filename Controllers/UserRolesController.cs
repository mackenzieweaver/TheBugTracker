using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TheBugTracker.Data;
using TheBugTracker.Models.ViewModels;
using TheBugTracker.Services.Interfaces;
using TheBugTracker.Extensions;
using TheBugTracker.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace TheBugTracker.Controllers
{
    [Authorize]
    public class UserRolesController : Controller
    {
        private readonly IBTRolesService _rolesService;
        private readonly IBTCompanyInfoService _companyInfoService;
        private readonly ApplicationDbContext _context;

        public UserRolesController(IBTRolesService rolesService, IBTCompanyInfoService companyInfoService, ApplicationDbContext context)
        {
            _rolesService = rolesService;
            _companyInfoService = companyInfoService;
            _context = context;
        }

        public async Task<IActionResult> Manage()
        {
            List<ManageUserRolesViewModel> model = new();
            int companyId = User.Identity.GetCompanyId().Value;
            List<BTUser> users = await _companyInfoService.GetAllMembersAsync(companyId);
            var roles = await _rolesService.GetRolesAsync();
            foreach (var user in users)
            {
                var userroles = await _rolesService.GetUserRolesAsync(user);
                model.Add(new ManageUserRolesViewModel{
                    BTUser = user,
                    Roles = new MultiSelectList(roles, "Name", "Name", userroles),
                    SelectedRoles = (List<string>)userroles
                });
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(string userId, List<string> selectedRoles)
        {
            var user = await _context.Users.FindAsync(userId);
            var roles = await _rolesService.GetUserRolesAsync(user);
            if(roles.AsQueryable().Count() > 0){
                await _rolesService.RemoveUserFromRolesAsync(user, roles);
            }
            foreach (var role in selectedRoles)
            {
                await _rolesService.AddUserToRoleAsync(user, role);
            }
            return RedirectToAction(nameof(Manage));
        }
    }
}
