using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheBugTracker.Data;
using TheBugTracker.Models;
using TheBugTracker.Services.Interfaces;
using TheBugTracker.Models.Enums;

namespace TheBugTracker.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;

        public BTProjectService(ApplicationDbContext context, IBTRolesService rolesService)
        {
            _context = context;
            _rolesService = rolesService;
        }

        public async Task AddNewProjectAsync(Project project)
        {
            await _context.AddAsync(project);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> AddProjectManagerAsync(string userId, int projectId)
        {
            await AddUserToProjectAsync(userId, projectId);
            BTUser user = await _context.Users.FindAsync(userId);
            return await _rolesService.AddUserToRoleAsync(user, Roles.ProjectManager.ToString());
        }

        public async Task<bool> AddUserToProjectAsync(string userId, int projectId)
        {
            BTUser user = await _context.Users.FindAsync(userId);
            if(user is null) return false;
            Project project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);
            if(project is null) return false;
            if(await IsUserOnProjectAsync(userId, projectId)) return true;
            try
            {
                project.Members.Add(user);
            }
            catch (System.NotSupportedException ex)
            {
                Console.WriteLine($"Error adding user {user.FullName} to {project.Name}: {ex}");
            }
            await UpdateProjectAsync(project);
            return true;
        }

        public async Task ArchiveProjectAsync(Project project)
        {
            project.Archived = true;
            await UpdateProjectAsync(project);

            foreach(var ticket in project.Tickets)
            {
                ticket.ArchivedByProject = true;
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
        }
       
        public async Task RestoreProjectAsync(Project project)
        {
            project.Archived = false;
            await UpdateProjectAsync(project);

            foreach(var ticket in project.Tickets)
            {
                ticket.ArchivedByProject = false;
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<BTUser>> GetAllProjectMembersExceptPMAsync(int projectId)
        {
            Project project = await _context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == projectId);
            List<BTUser> allMembersExceptPM = new();
            foreach(BTUser member in project.Members)
            {
                if(await _rolesService.IsUserInRoleAsync(member, Roles.ProjectManager.ToString())) continue;
                allMembersExceptPM.Add(member);
            }
            return allMembersExceptPM;
        }

        public async Task<List<Project>> GetAllProjectsByCompany(int companyId)
        {
            IQueryable<Project> query = _context.Projects
                .Where(p => p.CompanyId == companyId && p.Archived == false)
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .Include(p => p.Members)
                .Include(p => p.Tickets)
                .Include(p => p.Tickets).ThenInclude(t => t.Comments)
                .Include(p => p.Tickets).ThenInclude(t => t.Attachments)
                .Include(p => p.Tickets).ThenInclude(t => t.History)
                .Include(p => p.Tickets).ThenInclude(t => t.Notifications)
                .Include(p => p.Tickets).ThenInclude(t => t.DeveloperUser)
                .Include(p => p.Tickets).ThenInclude(t => t.OwnerUser)
                .Include(p => p.Tickets).ThenInclude(t => t.TicketStatus)
                .Include(p => p.Tickets).ThenInclude(t => t.TicketPriority)
                .Include(p => p.Tickets).ThenInclude(t => t.TicketType);
            return await query.ToListAsync();
        }

        public async Task<List<Project>> GetAllProjectsByPriority(int companyId, string priorityName)
        {
            List<Project> projects = await GetAllProjectsByCompany(companyId);
            IQueryable<Project> projectsByPriority = projects.Where(p => p.ProjectPriority.Name == priorityName).AsQueryable();
            return await projectsByPriority.ToListAsync();
        }

        public async Task<List<Project>> GetArchivedProjectsByCompany(int companyId)
        {
            List<Project> projects = await GetAllProjectsByCompany(companyId);
            IQueryable<Project> archived = projects.Where(p => p.Archived == true).AsQueryable();
            return await archived.ToListAsync();
        }

        public async Task<List<BTUser>> GetDevelopersOnProjectAsync(int projectId)
        {
            return await GetProjectMembersByRoleAsync(projectId, Roles.Developer.ToString());
        }

        public async Task<Project> GetProjectByIdAsync(int projectId, int companyId)
        {
            return await _context.Projects
                .Where(p => p.CompanyId == companyId)
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .Include(p => p.Members)
                .Include(p => p.Tickets)
                .FirstOrDefaultAsync(p => p.Id == projectId);
        }

        public async Task<BTUser> GetProjectManagerAsync(int projectId)
        {
            Project project = await _context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == projectId);
            if(project is null) return null;

            foreach(BTUser user in project.Members)
            {
                if(await _rolesService.IsUserInRoleAsync(user, Roles.ProjectManager.ToString())) return user;
            }
            return null;
        }

        public async Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string role)
        {
            Project project = await _context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == projectId);
            List<BTUser> members = project.Members.ToList();
            foreach(BTUser member in members)
            {
                if(!await _rolesService.IsUserInRoleAsync(member, role))
                {
                    members.Remove(member);
                }
            }
            return members;
        }

        public async Task<List<BTUser>> GetSubmittersOnProjectAsync(int projectId)
        {
            return await GetProjectMembersByRoleAsync(projectId, Roles.Submitter.ToString());
        }

        public async Task<List<Project>> GetUserProjectsAsync(string userId)
        {
            BTUser user = await _context.Users
                .Include(u => u.Projects)
                .Include(u => u.Projects).ThenInclude(p => p.Company)
                .Include(u => u.Projects).ThenInclude(p => p.Members)
                .Include(u => u.Projects).ThenInclude(p => p.Tickets)
                .Include(u => u.Projects).ThenInclude(p => p.Tickets).ThenInclude(t => t.DeveloperUser)
                .Include(u => u.Projects).ThenInclude(p => p.Tickets).ThenInclude(t => t.OwnerUser)
                .Include(u => u.Projects).ThenInclude(p => p.Tickets).ThenInclude(t => t.TicketPriority)
                .Include(u => u.Projects).ThenInclude(p => p.Tickets).ThenInclude(t => t.TicketStatus)
                .Include(u => u.Projects).ThenInclude(p => p.Tickets).ThenInclude(t => t.TicketType)
                .FirstOrDefaultAsync(u => u.Id == userId);
            return user.Projects.ToList();
        }

        public async Task<List<BTUser>> GetUsersNotOnProjectAsync(int projectId, int companyId)
        {
            var users = await _context.Users
                .Where(u => u.Projects.All(p => p.Id != projectId))
                .ToListAsync();
            return users.Where(u => u.CompanyId == companyId).ToList();
        }

        public async Task<bool> IsUserOnProjectAsync(string userId, int projectId)
        {
            Project project = await _context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == projectId);
            if(project is null) return false;
            
            return project.Members.Any(m => m.Id == userId);
        }

        public async Task<int> LookupProjectPriorityId(string priorityName)
        {
            ProjectPriority pp = await _context.ProjectPriorities
                .FirstOrDefaultAsync(pp => pp.Name == priorityName);
            return pp.Id;
        }

        public async Task RemoveProjectManagerAsync(int projectId)
        {
            Project project = await _context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == projectId);
            if(project is null) return;
            
            foreach(BTUser user in project.Members)
            {
                if(await _rolesService.IsUserInRoleAsync(user, Roles.ProjectManager.ToString()))
                {
                    try
                    {
                        await RemoveUserFromProjectAsync(user.Id, projectId);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"Error removing project manager {user.FullName}: {ex}");
                    }
                }
            }
        }

        public async Task RemoveUserFromProjectAsync(string userId, int projectId)
        {
            BTUser user = await _context.Users.FindAsync(userId);
            if(user is null) return;
            Project project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);
            if(project is null) return;

            if(project.Members.Any(m => m.Id == user.Id))
            {
                try
                {
                    project.Members.Remove(user);
                }
                catch(NotSupportedException ex)
                {
                    Console.WriteLine($"Error removing user {user.FullName} from project {project.Name}: {ex}");
                }
                await UpdateProjectAsync(project);
            }
        }

        public async Task RemoveUsersFromProjectByRoleAsync(string role, int projectId)
        {
            List<BTUser> members = await GetProjectMembersByRoleAsync(projectId, role);
            Project project = await _context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            foreach(BTUser member in members)
            {
                try
                {
                    project.Members.Remove(member);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error removing user {member.FullName} from project {project.Name}: {ex}");
                }
            }
            await UpdateProjectAsync(project);
        }

        public async Task UpdateProjectAsync(Project project)
        {
            try
            {
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating project {project.Name}: {ex}");
            }
        }
    }
}