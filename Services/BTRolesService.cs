using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheBugTracker.Data;
using TheBugTracker.Models;
using TheBugTracker.Services.Interfaces;

namespace TheBugTracker.Services
{
    public class BTRolesService : IBTRolesService
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<BTUser> _userManager;

        public BTRolesService(ApplicationDbContext context, RoleManager<IdentityRole> roleManager, UserManager<BTUser> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<bool> AddUserToRoleAsync(BTUser user, string roleName)
        {
            IdentityResult result = await _userManager.AddToRoleAsync(user, roleName);
            return result.Succeeded;
        }

        public async Task<string> GetRoleNameByIdAsync(string roleId)
        {
            IdentityRole role = await _context.Roles.FindAsync(roleId);
            return await _roleManager.GetRoleNameAsync(role);
        }

        public async Task<List<IdentityRole>> GetRolesAsync()
        {
            return await _context.Roles.ToListAsync();
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(BTUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId)
        {
            IList<BTUser> users = await _userManager.GetUsersInRoleAsync(roleName);
            IEnumerable<BTUser> usersInRoleInCompany = users.Where(u => u.CompanyId == companyId);
            return usersInRoleInCompany.ToList();
        }

        public async Task<List<BTUser>> GetUsersNotInRoleAsync(string roleName, int companyId)
        {
            IList<BTUser> usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
            List<string> usersInRoleIds = usersInRole.Select(u => u.Id).ToList();
            List<BTUser> usersNotInRole = await _context.Users.Where(u => !usersInRoleIds.Contains(u.Id)).ToListAsync();
            return usersNotInRole.Where(u => u.CompanyId == companyId).ToList();
        }

        public async Task<bool> IsUserInRoleAsync(BTUser user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async Task<bool> RemoveUserFromRoleAsync(BTUser user, string roleName)
        {
            IdentityResult result = await _userManager.RemoveFromRoleAsync(user, roleName);
            return result.Succeeded;
        }

        public async Task<bool> RemoveUserFromRolesAsync(BTUser user, IEnumerable<string> roles)
        {
            IdentityResult result = await _userManager.RemoveFromRolesAsync(user, roles);
            return result.Succeeded;
        }
    }
}
