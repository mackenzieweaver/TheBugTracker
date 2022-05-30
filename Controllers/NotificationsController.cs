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
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Index(string id)
        {
            var notifications = await _context.Notifications
                .Where(x => x.RecipientId == id)
                .Include(x => x.Ticket)
                .Include(x => x.Recipient)
                .Include(x => x.Sender)
                .ToListAsync();
            return View(notifications);
        }
        
        public async Task<IActionResult> MarkAllAsRead(string id)
        {
            var notifications = await _context.Notifications.Where(x => x.RecipientId == id).ToListAsync();
            foreach(var n in notifications)
            {
                n.Viewed = true;
                _context.Update(n);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .Include(n => n.Recipient)
                .Include(n => n.Sender)
                .Include(n => n.Ticket)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }
    }
}
