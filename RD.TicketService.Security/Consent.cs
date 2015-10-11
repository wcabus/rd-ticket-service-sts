namespace RD.TicketService.Security
{
    public class Consent
    {
        public string Client { get; set; }
        public string Subject { get; set; }
        public string ScopeList { get; set; }
    }
}