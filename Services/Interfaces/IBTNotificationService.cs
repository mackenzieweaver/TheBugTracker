using System.Collections.Generic;
using System.Threading.Tasks;
using TheBugTracker.Models;

namespace TheBugTracker.Services.Interfaces
{
    public interface IBTNotificationService
    {
        Task AddNotificationAsync(Notification notification);
        Task<List<Notification>> GetReceivedNotificationsAsync(string userId);
        Task<List<Notification>> GetSentNotificationsAsync(string userId);
        Task<bool> SendEmailNotificationAsync(Notification notification, string emailSubject);
        Task SendMembersEmailNotificationsByRoleAsync(Notification notification, int companyId, string role);
        Task SendMembersEmailNotificationsAsync(Notification notification, List<BTUser> members);
    }
}