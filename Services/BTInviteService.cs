using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TheBugTracker.Data;
using TheBugTracker.Models;
using TheBugTracker.Services.Interfaces;

namespace TheBugTracker.Services
{
    public class BTInviteService : IBTInviteService
    {
        private readonly ApplicationDbContext _context;

        public BTInviteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AcceptInviteAsync(Guid? token, string userId, int companyId)
        {
            var invite = await _context.Invites.FirstOrDefaultAsync(i => i.CompanyToken == token);
            if(invite is null) return false;
            invite.IsValid = false;
            invite.JoinDate = DateTime.Now;
            invite.InviteeId = userId;
            _context.Update(invite);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task AddNewInviteAsync(Invite invite)
        {
            await _context.AddAsync(invite);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> AnyInviteAsync(Guid token, string email, int companyId)
        {
            return await _context.Invites.Where(i => i.CompanyId == companyId)
                .AnyAsync( i => i.CompanyToken == token && i.InviteeEmail == email);
        }

        public async Task<Invite> GetInviteAsync(int inviteId, int companyId)
        {
            return await _context.Invites
                .Where(i => i.CompanyId == companyId)
                .Include(i => i.Company)
                .Include(i => i.Project)
                .Include(i => i.Invitor)
                .Include(i => i.Invitee)
                .FirstOrDefaultAsync(i => i.Id == inviteId);
        }

        public async Task<Invite> GetInviteAsync(Guid token, string email, int companyId)
        {
            return await _context.Invites
                .Where(i => i.CompanyId == companyId)
                .Where(i => i.CompanyToken == token)
                .Include(i => i.Company)
                .Include(i => i.Project)
                .Include(i => i.Invitor)
                .Include(i => i.Invitee)
                .FirstOrDefaultAsync(i => i.InviteeEmail == email);
        }

        public async Task<bool> ValidateInviteCodeAsync(Guid? token)
        {
            if(token is null) return false;
            Invite invite = await _context.Invites.FirstOrDefaultAsync(i => i.CompanyToken == token);

            if((DateTime.Now - invite.InviteDate.DateTime).TotalDays <= 7) 
            {
                invite.IsValid = false;
                _context.Update(invite);
                await _context.SaveChangesAsync();
            }
            return invite.IsValid;
        }
    }
}
