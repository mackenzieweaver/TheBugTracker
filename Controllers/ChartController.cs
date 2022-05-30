using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheBugTracker.Data;

namespace TheBugTracker.Controllers
{
    public class ChartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<JsonResult> TicketTypes()
        {
            var tickets = await _context.Tickets.ToListAsync();
            var types = tickets.GroupBy(x => x.TicketTypeId);
            Dictionary<string, int> list = new();
            foreach (var type in types)
            {
                var t = await _context.TicketTypes.FirstOrDefaultAsync(x => x.Id == type.Key);
                list.Add(t.Name, type.Count());
            }
            return Json(list);
        }
        
        public async Task<JsonResult> TicketStatuses()
        {
            var tickets = await _context.Tickets.ToListAsync();
            var statuses = tickets.GroupBy(x => x.TicketStatusId);
            Dictionary<string, int> list = new();
            foreach (var status in statuses)
            {
                var t = await _context.TicketStatuses.FirstOrDefaultAsync(x => x.Id == status.Key);
                list.Add(t.Name, status.Count());
            }
            return Json(list);
        }
        
        public async Task<JsonResult> TicketPriorities()
        {
            var tickets = await _context.Tickets.ToListAsync();
            var priorities = tickets.GroupBy(x => x.TicketPriorityId);
            Dictionary<string, int> list = new();
            foreach (var priority in priorities)
            {
                var t = await _context.TicketPriorities.FirstOrDefaultAsync(x => x.Id == priority.Key);
                list.Add(t.Name, priority.Count());
            }
            return Json(list);
        }
    }
}