using System.Collections.Generic;
using System.Threading.Tasks;
using TheBugTracker.Models;

namespace TheBugTracker.Services.Interfaces
{
    public interface IBTTicketHistoryService
    {
        Task AddHistoryAsync(Ticket oldticket, Ticket newticket, string userId);
        Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int projectId);
        Task<List<TicketHistory>> GetCompanyTicketsHistoriesAsync(int companyId);
    }
}