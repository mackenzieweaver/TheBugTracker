using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TheBugTracker.Data;
using TheBugTracker.Models;
using TheBugTracker.Services.Interfaces;

namespace TheBugTracker.Services
{
    public class BTCompanyInfoService : IBTCompanyInfoService
    {
        private readonly ApplicationDbContext _context;

        public BTCompanyInfoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<BTUser>> GetAllMembersAsync(int companyId)
        {
            IQueryable<BTUser> query = _context.Users.Where(u => u.CompanyId == companyId);
            return await query.ToListAsync();
        }

        public async Task<List<Project>> GetAllProjectsAsync(int companyId)
        {
            IQueryable<Project> query = _context.Projects
                .Where(p => p.CompanyId == companyId)
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

        public async Task<List<Ticket>> GetAllTicketsAsync(int companyId)
        {
            List<Project> projects = await GetAllProjectsAsync(companyId);
            IQueryable<Ticket> ticketsQuery = projects.SelectMany(p => p.Tickets).AsQueryable();
            return await ticketsQuery.ToListAsync();
        }

        public async Task<Company> GetCompanyInfoByIdAsync(int? companyId)
        {
            return await _context.Companies
                .Include(c => c.Members)
                .Include(c => c.Projects)
                .Include(c => c.Invites)
                .FirstOrDefaultAsync(c => c.Id == companyId);
        }
    }
}
