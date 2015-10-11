namespace RD.TicketService.Security
{
    public class ExternalLoginInfo
    {
        public ExternalLoginInfo()
        {
            
        }

        public ExternalLoginInfo(string provider, string nameIdentifier)
        {
            Provider = provider;
            NameIdentifier = nameIdentifier;
        }

        public string Provider { get; set; } 
        public string NameIdentifier { get; set; }
    }
}