using System.Collections.Generic;

namespace TheBugTracker.Models
{
    public class CallerViewModel
    {
        public string CallerId { get; set; }
        public List<BTUser> Users { get; set; }
    }
}