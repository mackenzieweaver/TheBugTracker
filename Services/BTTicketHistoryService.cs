using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TheBugTracker.Data;
using TheBugTracker.Models;
using TheBugTracker.Services.Interfaces;
using System.Reflection;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace TheBugTracker.Services
{
    public class BTTicketHistoryService : IBTTicketHistoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTTicketService _ticketService;

        public BTTicketHistoryService(ApplicationDbContext context, IBTTicketService ticketService)
        {
            _context = context;
            _ticketService = ticketService;
        }

        public Ticket DeepCopyTicket(Ticket ticket)
        {
            var oldTicket = new Ticket();
            Type type = oldTicket.GetType();
            foreach(var prop in type.GetProperties())
            {
                var val = prop.GetValue(ticket);
                prop.SetValue(oldTicket, val);
            }
            return oldTicket;
        }

        public async Task AddHistoryAsync(Ticket oldticket, Ticket newticket, string userId)
        {
            if(oldticket is null && newticket is not null)
            {
                var history = new TicketHistory
                {
                    TicketId = newticket.Id,
                    Created = DateTime.Now,
                    UserId = userId,
                    Description = "Ticket created"
                };
                await _context.AddAsync(history);
            }
            else
            {
                Type t = new Ticket().GetType();
                PropertyInfo[] props = t.GetProperties();
                foreach(var prop in props)
                {                        
                    if(prop.PropertyType.FullName.Contains("TheBugTracker")) continue;
                    if(prop.PropertyType.FullName.Contains("ICollection")) continue;
                    
                    var property = t.GetProperty(prop.Name);
                    var oldval = property.GetValue(oldticket);
                    if(oldval is null) continue;
                    var newval = property.GetValue(newticket);
                    if(newval is null) continue;

                    if(!oldval.Equals(newval))
                    {
                        var history = new TicketHistory
                        {
                            TicketId = newticket.Id,
                            Created = DateTime.Now,
                            UserId = userId,
                            Property = prop.Name,
                            OldValue = oldval.ToString(),
                            NewValue = newval.ToString(),
                            Description = $"{prop.Name} changed from {oldval} to {newval}"
                        };
                        await _context.AddAsync(history);
                    }
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task<List<TicketHistory>> GetCompanyTicketsHistoriesAsync(int companyId)
        {
            List<Ticket> tickets = await _ticketService.GetAllTicketsByCompanyAsync(companyId);
            return tickets.SelectMany(t => t.History).ToList();
        }

        public async Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int projectId)
        {
            Project project = await _context.Projects
                .Include(p => p.Tickets).ThenInclude(t => t.History).ThenInclude(h => h.User)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            IQueryable<TicketHistory> histories = project.Tickets.SelectMany(t => t.History).AsQueryable();
            return await histories.ToListAsync();
        }
    }
}
