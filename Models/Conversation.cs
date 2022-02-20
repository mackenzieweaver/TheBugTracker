using System.Collections.Generic;

namespace TheBugTracker.Models
{
    public class Conversation
    {
        public BTUser User { get; set; }
        public List<Message> Messages { get; set; }
    }
}