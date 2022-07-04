using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PasswordGenerator;
using TheBugTracker.Data;
using TheBugTracker.Models;
using TheBugTracker.Models.Enums;
using TheBugTracker.Services;
using TheBugTracker.Services.Interfaces;

namespace TheBugTracker.Controllers
{
    [Authorize]
    public class InvitesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTRolesService _rolesService;
        private readonly SignInManager<BTUser> _signInManager;

        public InvitesController(ApplicationDbContext context, IEmailSender emailService, UserManager<BTUser> userManager, IBTRolesService rolesService, SignInManager<BTUser> signInManager)
        {
            _emailService = emailService;
            _context = context;
            _userManager = userManager;
            _rolesService = rolesService;
            _signInManager = signInManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Invites.Include(i => i.Company).Include(i => i.Invitee).Include(i => i.Invitor).Include(i => i.Project);
            return View(await applicationDbContext.ToListAsync());
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invite = await _context.Invites
                .Include(i => i.Company)
                .Include(i => i.Invitee)
                .Include(i => i.Invitor)
                .Include(i => i.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invite == null)
            {
                return NotFound();
            }

            return View(invite);
        }

        public async Task<IActionResult> Create()
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);
            ViewData["CompanyId"] = user.CompanyId;
            ViewData["ProjectId"] = new SelectList(_context.Projects.Where(x => x.CompanyId == user.CompanyId), "Id", "Name");
            return View();
        }
        
        [AllowAnonymous]
        public async Task<IActionResult> Accept(int id)
        {
            Invite i = await _context.Invites
                .Include(x => x.Invitor)
                .Include(x => x.Company)
                .FirstOrDefaultAsync(x => x.Id == id);
            if(i is null) return NotFound();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == i.InviteeEmail);
            var pw = new Password();
            string password = pw.Next();
            
            if(user is null){
                user = new BTUser {                  
                    UserName = i.InviteeEmail,
                    FirstName = i.InviteeFirstName,
                    LastName = i.InviteeLastName,
                    Email = i.InviteeEmail,
                    CompanyId = i.CompanyId
                };
                var result = await _userManager.CreateAsync(user, password);
                await _rolesService.AddUserToRoleAsync(user, Roles.Developer.ToString());
            }

            if(_signInManager.IsSignedIn(User)){
                await _signInManager.SignOutAsync();
            }
            await _signInManager.SignInAsync(user, isPersistent: false);

            ViewData["invitor"] = i.Invitor.FullName;
            ViewData["username"] = user.Email;
            ViewData["password"] = password;
            ViewData["company"] = i.Company.Name;
            return View();
        }
       
        public IActionResult Sent()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Invite invite)
        {
            invite.CompanyToken = Guid.NewGuid();
            await _context.AddAsync(invite);
            await _context.SaveChangesAsync();

            var company = await _context.Companies.FindAsync(invite.CompanyId);
            var username = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == username);
            var callbackUrl = $"{Request.Scheme}://{Request.Host}/Invites/Accept?id={invite.Id}";
            var registerUrl = $"{Request.Scheme}://{Request.Host}/Identity/Account/Register";
            
            var subject = "Invitation to The Bug Tracker";
            var body = $"{user.FullName} has invited you to join their company {company.Name}.<br>";
            body += $"<a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Click here to accept and join</a>.<br><br>";

            body += $"Optionally you can go <a href='{HtmlEncoder.Default.Encode(registerUrl)}'>here</a> to register your account and use this code to join the company.<br>";
            body += $"Your code: {invite.CompanyToken}";
            await _emailService.SendEmailAsync( invite.InviteeEmail, subject, body );
            return RedirectToAction("Sent");
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invite = await _context.Invites.FindAsync(id);
            if (invite == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", invite.CompanyId);
            ViewData["InviteeId"] = new SelectList(_context.Users, "Id", "Id", invite.InviteeId);
            ViewData["InvitorId"] = new SelectList(_context.Users, "Id", "Id", invite.InvitorId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", invite.ProjectId);
            return View(invite);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,InviteDate,JoinDate,CompanyToken,CompanyId,ProjectId,InvitorId,InviteeId,InviteeEmail,InviteeFirstName,InviteeLastName,IsValid")] Invite invite)
        {
            if (id != invite.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(invite);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InviteExists(invite.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", invite.CompanyId);
            ViewData["InviteeId"] = new SelectList(_context.Users, "Id", "Id", invite.InviteeId);
            ViewData["InvitorId"] = new SelectList(_context.Users, "Id", "Id", invite.InvitorId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", invite.ProjectId);
            return View(invite);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invite = await _context.Invites
                .Include(i => i.Company)
                .Include(i => i.Invitee)
                .Include(i => i.Invitor)
                .Include(i => i.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invite == null)
            {
                return NotFound();
            }

            return View(invite);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var invite = await _context.Invites.FindAsync(id);
            _context.Invites.Remove(invite);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InviteExists(int id)
        {
            return _context.Invites.Any(e => e.Id == id);
        }
    }
}
