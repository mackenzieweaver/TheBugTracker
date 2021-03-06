using System.Collections.Generic;

namespace TheBugTracker.Models
{
    public class UserProfileViewModel
    {
        public BTUser User { get; set; }
        public List<Ticket> TicketsSubmitted { get; set; }
        public List<Ticket> TicketsAssigned { get; set; }
    }
}