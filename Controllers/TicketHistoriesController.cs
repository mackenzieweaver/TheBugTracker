using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TheBugTracker.Data;
using TheBugTracker.Models;

namespace TheBugTracker.Controllers
{
    public class TicketHistoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TicketHistoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TicketHistories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketHistory = await _context.TicketHistories
                .Include(t => t.Ticket)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketHistory == null)
            {
                return NotFound();
            }

            return View(ticketHistory);
        }
    }
}
