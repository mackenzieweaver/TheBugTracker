using System;

namespace TheBugTracker.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTimeOffset Created { get; set; }
        public bool Seen { get; set; }
        public string FromUserId { get; set; }
        public string ToUserId { get; set; }
    }
}
