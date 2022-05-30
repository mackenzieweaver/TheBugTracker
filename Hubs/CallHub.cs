using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TheBugTracker.Data;
using TheBugTracker.Models;

namespace TheBugTracker.Hubs
{
    public class CallHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public CallHub(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task SendOfferToUser(string id, string offer, string callerId)
        {
            await Clients.User(id).SendAsync("ReceiveOffer", offer, callerId);
        }

        public async Task SendAnswerToCaller(string id, string answer)
        {
            await Clients.User(id).SendAsync("ReceiveAnswer", answer);
        }
        
        public async Task SendIceCandidate(string id, string iceCandidate)
        {
            await Clients.User(id).SendAsync("ReceiveIceCandidate", iceCandidate);
        }
        
        public async Task Test()
        {
            await Clients.All.SendAsync("Test");
        }
        
        public async Task NotifyCallee(string calleeId, string callerId, string url)
        {
            var callee = await _context.Users.FindAsync(calleeId);
            var caller = await _context.Users.FindAsync(callerId);
            await Clients.User(calleeId).SendAsync("IncomingCall", caller.FirstName, url);
        }
        
        public async Task CalleeReady(string callerId)
        {
            await Clients.User(callerId).SendAsync("CalleeReady");
        }
    }
}
