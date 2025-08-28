namespace TestTools.Models
{
    public class ApiEndpointModel
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Status { get; set; } // Active, Slow, Unavailable
        public string StatusDetail { get; set; } // e.g. HTTP status, error
        public int ResponseTimeMs { get; set; }
        public bool Active { get; set; } // "true" or "false"
    }
}
