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
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == Context.User.Identity.Name);
            var m = new Message
            {
                Text = message,
                Created = DateTime.Now,
                FromUserId = user.Id,
                ToUserId = id
            };
            await _context.Messages.AddAsync(m);
            await _context.SaveChangesAsync();

            await Clients.User(id).SendAsync("ReceivePrivateMessage", message);
        }
    }
}