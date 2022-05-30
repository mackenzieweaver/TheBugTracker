using System;
using System.Collections.Generic;

namespace TheBugTracker.Models
{
    public class Job
    {
        public int id { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public string company_name { get; set; }
        public string category { get; set; }
        public List<object> tags { get; set; }
        public string job_type { get; set; }
        public DateTime publication_date { get; set; }
        public string candidate_required_location { get; set; }
        public string salary { get; set; }
        public string description { get; set; }
        public string company_logo_url { get; set; }
    }
}