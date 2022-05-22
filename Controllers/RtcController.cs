using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheBugTracker.Data;
using TheBugTracker.Models;

namespace TheBugTracker.Controllers
{
    [Authorize]
    public class RtcController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RtcController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);
            var users = await _context.Users.ToListAsync();
            users.Remove(user);
            var viewModel = new CallerViewModel 
            {
                CallerId = user.Id,
                Users = users
            };
            return View(viewModel);
        }
        
        public async Task<IActionResult> Call(string calleeid, string callid, string callerid)
        {
            var callee = await _context.Users.FindAsync(calleeid);
            var caller = await _context.Users.FindAsync(callerid);
            return View(new Call {
                Callee = callee,
                Caller = caller,
                CallId = callid
            });
        }
    }
}
