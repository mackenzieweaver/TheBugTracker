using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TheBugTracker.Data;
using TheBugTracker.Models;

namespace TheBugTracker.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
        
        public async Task SendPrivateMessage(string id, string message)
        {
            var fromuser = await _context.Users.FirstOrDefaultAsync(x => x.UserName == Context.User.Identity.Name);
            var touser = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);

            var m = new Message
            {
                Text = message,
                Created = DateTime.Now,
                FromUserId = fromuser.Id,
                ToUserId = touser.Id
            };
            await _context.Messages.AddAsync(m);
            await _context.SaveChangesAsync();
            await Clients.User(id).SendAsync("ReceivePrivateMessage", message);
           
            var t = $"At {DateTime.Now.ToString("t")} on {DateTime.Now.ToString("d")}";
            var n = new Notification
            {
                ReturnUrl = $"/Chat/PrivateMessage/{fromuser.Id}",
                Title = $"New message from {fromuser.FullName}", 
                Message = message,
                Created = DateTime.Now,
                RecipientId = touser.Id,
                SenderId = fromuser.Id
            };
            await _context.Notifications.AddAsync(n);
            await _context.SaveChangesAsync();
            await Clients.User(id).SendAsync("ReceiveNotification", n.Id, n.ReturnUrl, n.Title, n.Message, t);
        }
        
        public async Task RemoveNotificationFromDb(int id)
        {
            var n = await _context.Notifications.FindAsync(id);
            _context.Remove(n);
            await _context.SaveChangesAsync();
        }
        
        public async Task MarkNotificationAsRead(int id)
        {
            var n = await _context.Notifications.FindAsync(id);
            n.Viewed = true;
            _context.Update(n);
            await _context.SaveChangesAsync();
        }
    }
}