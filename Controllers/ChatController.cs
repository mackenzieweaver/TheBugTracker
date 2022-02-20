using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheBugTracker.Data;
using TheBugTracker.Models;

namespace TheBugTracker.Controllers
{
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChatController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();            
            var user = users.Find(x => x.UserName == User.Identity.Name);
            users.Remove(user);
            users = users.Where(x => x.CompanyId == user.CompanyId).ToList();
            return View(users);
        }
        
        public async Task<IActionResult> PrivateMessage(string id)
        {
            var loggedInUser = await _context.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);
            var toUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);  
            var messages = await _context.Messages
                .Where(x => x.FromUserId == loggedInUser.Id && x.ToUserId == id || x.ToUserId == loggedInUser.Id && x.FromUserId == toUser.Id)
                .ToListAsync();
            
            var conversation = new Conversation 
            {
                User = toUser,
                Messages = messages
            };
            return View(conversation);
        }

        public IActionResult Anonymous()
        {
            return View();
        }
    }
}
