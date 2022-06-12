using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using TheBugTracker.Models;

namespace TheBugTracker.Services.Interfaces
{
    public interface IBTRolesService
    {
        Task<bool> IsUserInRoleAsync(BTUser user, string roleName);
        Task<List<IdentityRole>> GetRolesAsync();
        Task<IEnumerable<string>> GetUserRolesAsync(BTUser user);
        Task<bool> AddUserToRoleAsync(BTUser user, string roleName);
        Task<bool> RemoveUserFromRoleAsync(BTUser user, string roleName);
        Task<bool> RemoveUserFromRolesAsync(BTUser user, IEnumerable<string> roles);
        Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId);
        Task<List<BTUser>> GetUsersNotInRoleAsync(string roleName, int companyId);
        Task<string> GetRoleNameByIdAsync(string roleId);
    }
}
