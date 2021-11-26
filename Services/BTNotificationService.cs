using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using TheBugTracker.Data;
using TheBugTracker.Models;
using TheBugTracker.Services.Interfaces;

namespace TheBugTracker.Services
{
    public class BTNotificationService : IBTNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IBTRolesService _rolesService;

        public BTNotificationService(ApplicationDbContext context, IEmailSender emailSender, IBTRolesService rolesService)
        {
            _context = context;
            _emailSender = emailSender;
            _rolesService = rolesService;
        }

        public async Task AddNotificationAsync(Notification notification)
        {
            await _context.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetReceivedNotificationsAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.RecipientId == userId)
                .Include(n => n.Ticket).ThenInclude(t => t.Project)
                .Include(n => n.Recipient)
                .Include(n => n.Sender)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetSentNotificationsAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.SenderId == userId)
                .Include(n => n.Ticket).ThenInclude(t => t.Project)
                .Include(n => n.Recipient)
                .Include(n => n.Sender)
                .ToListAsync();
        }

        public async Task<bool> SendEmailNotificationAsync(Notification notification, string emailSubject)
        {
            BTUser user = await _context.Users.FindAsync(notification.RecipientId);
            if(user is null) return false;
            await _emailSender.SendEmailAsync(user.Email, emailSubject, notification.Message);
            return true;
        }

        public async Task SendMembersEmailNotificationsByRoleAsync(Notification notification, int companyId, string role)
        {
            List<BTUser> usersInRole = await _rolesService.GetUsersInRoleAsync(role, companyId);
            await SendMembersEmailNotificationsAsync(notification, usersInRole);
        }

        public async Task SendMembersEmailNotificationsAsync(Notification notification, List<BTUser> members)
        {
            foreach(BTUser user in members)
            {
                await SendEmailNotificationAsync(notification, notification.Title);
            }
        }
    }
}
