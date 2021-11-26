using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TheBugTracker.Data;
using TheBugTracker.Models;
using TheBugTracker.Models.Enums;
using TheBugTracker.Services.Interfaces;

namespace TheBugTracker.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTProjectService _projectService;
        private readonly IBTRolesService _rolesService;

        public BTTicketService(ApplicationDbContext context, IBTProjectService projectService, IBTRolesService rolesService)
        {
            _context = context;
            _projectService = projectService;
            _rolesService = rolesService;
        }

        public async Task AddNewTicketAsync(Ticket ticket)
        {
            await _context.AddAsync(ticket);
            await _context.SaveChangesAsync();
        }

        public async Task ArchiveTicketAsync(Ticket ticket)
        {
            ticket.Archived = true;
            await UpdateTicketAsync(ticket);
        }

        public async Task AssignTicketAsync(int ticketId, string userId)
        {
            Ticket ticket = await GetTicketByIdAsync(ticketId);
            ticket.DeveloperUserId = userId;
            await UpdateTicketAsync(ticket);
        }

        public async Task<List<Ticket>> GetAllTicketsByCompanyAsync(int companyId)
        {
            List<Project> projects = await _projectService.GetAllProjectsByCompany(companyId);
            List<Ticket> tickets = projects.SelectMany(p => p.Tickets).ToList();
            return tickets;
        }

        public async Task<List<Ticket>> GetAllTicketsByPriorityAsync(int companyId, string priorityName)
        {
            int? priorityId = await LookupTicketPriorityIdAsync(priorityName);
            List<Ticket> tickets = await GetAllTicketsByCompanyAsync(companyId);            
            tickets = tickets.Where(t => t.TicketPriorityId == priorityId).ToList();
            return tickets;
        }

        public async Task<List<Ticket>> GetAllTicketsByStatusAsync(int companyId, string statusName)
        {
            int? statusId = await LookupTicketStatusIdAsync(statusName);
            List<Ticket> tickets = await GetAllTicketsByCompanyAsync(companyId);            
            tickets = tickets.Where(t => t.TicketStatusId == statusId).ToList();
            return tickets;
        }

        public async Task<List<Ticket>> GetAllTicketsByTypeAsync(int companyId, string typeName)
        {
            int? typeId = await LookupTicketTypeIdAsync(typeName);
            List<Ticket> tickets = await GetAllTicketsByCompanyAsync(companyId);            
            tickets = tickets.Where(t => t.TicketTypeId == typeId).ToList();
            return tickets;
        }

        public async Task<List<Ticket>> GetArchivedTicketsAsync(int companyId)
        {
            List<Ticket> tickets = await GetAllTicketsByCompanyAsync(companyId);            
            tickets = tickets.Where(t => t.Archived == true).ToList();
            return tickets;
        }

        private async Task<List<Ticket>> GetProjectTickets(int projectId, int companyId)
        {
            List<Ticket> tickets = await GetAllTicketsByCompanyAsync(companyId);
            return tickets.Where(t => t.ProjectId == projectId).ToList();
        }

        public async Task<List<Ticket>> GetProjectTicketsByPriorityAsync(string priorityName, int companyId, int projectId)
        {
            List<Ticket> tickets = await GetProjectTickets(projectId, companyId);
            int? priorityId = await LookupTicketPriorityIdAsync(priorityName);
            if(priorityId is null) return null;
            return tickets.Where(t => t.TicketPriorityId == priorityId).ToList();
        }

        public async Task<List<Ticket>> GetProjectTicketsByRoleAsync(string role, string userId, int projectId, int companyId)
        {
            List<Ticket> tickets = await GetProjectTickets(projectId, companyId);
            return await TicketsByRoleAsync(userId, role, tickets);
        }

        public async Task<List<Ticket>> GetProjectTicketsByStatusAsync(string statusName, int companyId, int projectId)
        {
            List<Ticket> tickets = await GetProjectTickets(projectId, companyId);
            int? statusId = await LookupTicketStatusIdAsync(statusName);
            if(statusId is null) return null;
            return tickets.Where(t => t.TicketStatusId == statusId).ToList();
        }

        public async Task<List<Ticket>> GetProjectTicketsByTypeAsync(string typeName, int companyId, int projectId)
        {
            List<Ticket> tickets = await GetProjectTickets(projectId, companyId);
            int? typeId = await LookupTicketTypeIdAsync(typeName);
            if(typeId is null) return null;
            return tickets.Where(t => t.TicketTypeId == typeId).ToList();
        }

        private async Task<List<Ticket>> TicketsByRoleAsync(string userId, string role, List<Ticket> tickets)
        => role switch
        {
            "Developer" => tickets.Where(t => t.DeveloperUserId == userId).ToList(),
            "Submitter" => tickets.Where(t => t.OwnerUserId == userId).ToList(),
            "ProjectManager" => (await _projectService.GetUserProjectsAsync(userId)).SelectMany(p => p.Tickets).ToList(),
            _ => tickets
        };

        public async Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            return await _context.Tickets
                .Include(t => t.Project)
                .Include(t => t.TicketType)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.OwnerUser)
                .Include(t => t.DeveloperUser)
                .Include(t => t.Comments)
                .Include(t => t.Attachments)
                .Include(t => t.Notifications)
                .Include(t => t.History)
            .FirstOrDefaultAsync(t => t.Id == ticketId);
        }

        public async Task<BTUser> GetTicketDeveloperAsync(int ticketId)
        {
            Ticket ticket = await _context.Tickets
                .Include(t => t.DeveloperUser)
                .FirstOrDefaultAsync(t => t.Id == ticketId);
            return ticket.DeveloperUser;
        }

        public async Task<List<Ticket>> GetTicketsByRoleAsync(string role, string userId, int companyId)
        {
            BTUser user = await _context.Users.FindAsync(userId);
            List<Ticket> tickets = await GetTicketsByUserIdAsync(userId, companyId);
            return await TicketsByRoleAsync(userId, role, tickets);
        }

        public async Task<List<Ticket>> GetTicketsByUserIdAsync(string userId, int companyId)
        {
            List<Ticket> tickets = await GetAllTicketsByCompanyAsync(companyId);
            BTUser user = await _context.Users.FindAsync(userId);
            if(await _rolesService.IsUserInRoleAsync(user, Roles.Admin.ToString())) return tickets;
            return tickets.Where(t => t.OwnerUserId == userId || t.DeveloperUserId == userId).ToList();
        }

        public async Task<int?> LookupTicketPriorityIdAsync(string priorityName)
        {
            TicketPriority tp = await _context.TicketPriorities.FirstOrDefaultAsync(tp => tp.Name == priorityName);
            return tp?.Id;
        }

        public async Task<int?> LookupTicketStatusIdAsync(string statusName)
        {
            TicketStatus ts = await _context.TicketStatuses.FirstOrDefaultAsync(ts => ts.Name == statusName);
            return ts?.Id;
        }

        public async Task<int?> LookupTicketTypeIdAsync(string typeName)
        {
            TicketType tt = await _context.TicketTypes.FirstOrDefaultAsync(tt => tt.Name == typeName);
            return tt?.Id;
        }

        public async Task UpdateTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating ticket {ticket.Title}: {ex}");
            }
        }
    }
}