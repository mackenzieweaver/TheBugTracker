namespace TheBugTracker.Models
{
    public class Call
    {
        public BTUser Callee { get; set; }
        public BTUser Caller { get; set; }
        public string CallId { get; set; }
    }
}