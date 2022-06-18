using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TheBugTracker.Data;
using TheBugTracker.Models;
using TheBugTracker.Services.Interfaces;

namespace TheBugTracker.Services
{
    public class BTLookupService : IBTLookupService
    {
        private readonly ApplicationDbContext _context;

        public BTLookupService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TicketPriority>> GetTicketPrioritiesAsync()
        {
            return await _context.TicketPriorities.ToListAsync();
        }
        public async Task<List<TicketStatus>> GetTicketStatusesAsync()
        {
            return await _context.TicketStatuses.ToListAsync();             
        }
        public async Task<List<TicketType>> GetTicketTypesAsync()
        {
            return await _context.TicketTypes.ToListAsync();            
        }
        public async Task<List<ProjectPriority>> GetProjectPrioritiesAsync()
        {
            return await _context.ProjectPriorities.ToListAsync();            
        }
    }
}
