using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TheBugTracker.Data;
using TheBugTracker.Models;
using TheBugTracker.Services.Interfaces;

namespace TheBugTracker.Controllers
{
    [Authorize]
    public class TicketStatusesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTTicketHistoryService _historyService;

        public TicketStatusesController(ApplicationDbContext context, IBTTicketHistoryService historyService)
        {
            _context = context;
            _historyService = historyService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return View(await _context.TicketStatuses.ToListAsync());
        }

        // GET: TicketStatuses/Create
        public IActionResult Create()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] TicketStatus ticketStatus)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ticketStatus);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ticketStatus);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int ticketId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            var oldTicket = _historyService.DeepCopyTicket(ticket);

            ticket.TicketStatusId = id;
            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);
            await _historyService.AddHistoryAsync(oldTicket, ticket, user.Id);
            return RedirectToAction("Details", "Tickets", new { id = ticketId });
        }
        
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketStatus = await _context.TicketStatuses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketStatus == null)
            {
                return NotFound();
            }

            return View(ticketStatus);
        }
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticketStatus = await _context.TicketStatuses.FindAsync(id);
            _context.TicketStatuses.Remove(ticketStatus);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketStatusExists(int id)
        {
            return _context.TicketStatuses.Any(e => e.Id == id);
        }
    }
}
