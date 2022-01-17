using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TheBugTracker.Data;
using TheBugTracker.Models;
using TheBugTracker.Services.Interfaces;

namespace TheBugTracker.Controllers
{
    public class TicketStatusesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTTicketHistoryService _historyService;

        public TicketStatusesController(ApplicationDbContext context, IBTTicketHistoryService historyService)
        {
            _context = context;
            _historyService = historyService;
        }

        // GET: TicketStatuses
        public async Task<IActionResult> Index()
        {
            return View(await _context.TicketStatuses.ToListAsync());
        }

        // GET: TicketStatuses/Details/5
        public async Task<IActionResult> Details(int? id)
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

        // GET: TicketStatuses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TicketStatuses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: TicketStatuses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketStatus = await _context.TicketStatuses.FindAsync(id);
            if (ticketStatus == null)
            {
                return NotFound();
            }
            return View(ticketStatus);
        }

        // POST: TicketStatuses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: TicketStatuses/Delete/5
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

        // POST: TicketStatuses/Delete/5
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
