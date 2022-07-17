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
    public class TicketAttachmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTFileService _fileService;

        public TicketAttachmentsController(ApplicationDbContext context, IBTFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TicketId,Created,UserId,Description,FileName,FileData,FormFile,FileContentType")] TicketAttachment ticketAttachment)
        {
            ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
            ticketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
            ticketAttachment.FileContentType = ticketAttachment.FormFile.ContentType;
            if (ModelState.IsValid)
            {
                _context.Add(ticketAttachment);
                await _context.SaveChangesAsync();
            }            
            return RedirectToAction("Details", "Tickets", new { id = ticketAttachment.TicketId });
        }
        
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticketAttachment = await _context.TicketAttachments
                .Include(t => t.Ticket)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticketAttachment == null)
            {
                return NotFound();
            }

            return View(ticketAttachment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticketAttachment = await _context.TicketAttachments.FindAsync(id);
            _context.TicketAttachments.Remove(ticketAttachment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Tickets", new{ id = ticketAttachment.TicketId });
        }

        private bool TicketAttachmentExists(int id)
        {
            return _context.TicketAttachments.Any(e => e.Id == id);
        }
    }
}
