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
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTTicketHistoryService _historyService;

        public TicketsController(ApplicationDbContext context, IBTTicketHistoryService historyService)
        {
            _context = context;
            _historyService = historyService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var tickets = await _context.Tickets
                .Include(t => t.DeveloperUser)
                .Include(t => t.OwnerUser)
                .Include(t => t.Project)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .ToListAsync();
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);
            if(user is not null) tickets = tickets.Where(ticket => ticket.Project.CompanyId == user.CompanyId).ToList();
            return View(tickets);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.DeveloperUser)
                .Include(t => t.OwnerUser)
                .Include(t => t.Project)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .Include(t => t.Attachments)
                .Include(t => t.Comments).ThenInclude(x => x.User)
                .Include(t => t.History).ThenInclude(x => x.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ticket == null) return NotFound();
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);
            if(user is not null)
            {
                var projects = await _context.Projects.ToListAsync();
                var project = projects.FirstOrDefault(x => x.Id == ticket.ProjectId);
                if(project.CompanyId != user.CompanyId) return NotFound();
            }

            ViewData["StatusList"] = new SelectList(_context.TicketStatuses.Where(x => x.Id != ticket.TicketStatusId), "Id", "Name");
            ViewData["PriorityList"] = new SelectList(_context.TicketPriorities.Where(x => x.Id != ticket.TicketPriorityId), "Id", "Name");
            ViewData["TypeList"] = new SelectList(_context.TicketTypes.Where(x => x.Id != ticket.TicketTypeId), "Id", "Name");
            ViewData["AssigneeList"] = new SelectList(_context.Users.Where(x => x.Id != ticket.DeveloperUserId), "Id", "FullName");
            return View(ticket);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "FullName");
            ViewData["OwnerUserId"] = new SelectList(_context.Users, "Id", "FullName");
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name");
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name");
            ViewData["TicketStatusId"] = (await _context.TicketStatuses.FirstAsync()).Id;
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Created,Updated,Archived,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,OwnerUserId,DeveloperUserId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                await _historyService.AddHistoryAsync(null, ticket, ticket.OwnerUserId);
                return RedirectToAction(nameof(Index));
            }
            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
            ViewData["OwnerUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.OwnerUserId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", ticket.ProjectId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Id", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Id", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Id", ticket.TicketTypeId);
            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDeveloper(string id, int ticketId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            var oldTicket = _historyService.DeepCopyTicket(ticket);

            ticket.DeveloperUserId = id;
            ticket.Updated = DateTime.Now;
            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);
            await _historyService.AddHistoryAsync(oldTicket, ticket, user.Id);
            return RedirectToAction("Details", "Tickets", new { id = ticketId });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDescription(int id, string description)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            var oldTicket = _historyService.DeepCopyTicket(ticket);

            ticket.Description = description;
            ticket.Updated = DateTime.Now;
            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();
            
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);
            await _historyService.AddHistoryAsync(oldTicket, ticket, user.Id);
            return RedirectToAction("Details", "Tickets", new { id = id });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTitle(int id, string title)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            var oldTicket = _historyService.DeepCopyTicket(ticket);
            
            ticket.Title = title;
            ticket.Updated = DateTime.Now;
            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);
            await _historyService.AddHistoryAsync(oldTicket, ticket, user.Id);
            return RedirectToAction("Details", "Tickets", new { id = id });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditArchived(int id, bool archive)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            var oldTicket = _historyService.DeepCopyTicket(ticket);
            
            ticket.Archived = archive;
            ticket.Updated = DateTime.Now;
            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);
            await _historyService.AddHistoryAsync(oldTicket, ticket, user.Id);
            return RedirectToAction("Details", "Tickets", new { id = id });
        }
    }
}
