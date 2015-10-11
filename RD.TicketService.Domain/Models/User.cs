using RD.TicketService.Security;

namespace RD.TicketService.Models
{
    public class User : IUser<int>
    {
        public int Id { get; set; }

        public string UserName { get; set; }
    }
}