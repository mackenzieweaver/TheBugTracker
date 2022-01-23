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
    public class TicketTypesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTTicketHistoryService _historyService;

        public TicketTypesController(ApplicationDbContext context, IBTTicketHistoryService historyService)
        {
            _context = context;
            _historyService = historyService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return View(await _context.TicketTypes.ToListAsync());
        }

        // GET: TicketTypes/Create
        public IActionResult Create()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] TicketType ticketType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ticketType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ticketType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int ticketId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            var oldTicket = _historyService.DeepCopyTicket(ticket);

            ticket.TicketTypeId = id;
            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);
            await _historyService.AddHistoryAsync(oldTicket, ticket, user.Id);
            return RedirectToAction("Details", "Tickets", new { id = ticketId });
        }
        
        // GET: TicketTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketType = await _context.TicketTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketType == null)
            {
                return NotFound();
            }

            return View(ticketType);
        }

        // POST: TicketTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticketType = await _context.TicketTypes.FindAsync(id);
            _context.TicketTypes.Remove(ticketType);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketTypeExists(int id)
        {
            return _context.TicketTypes.Any(e => e.Id == id);
        }
    }
}
