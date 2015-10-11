namespace RD.TicketService.Security.Samples
{
    public class User : IUser<string>
    {
        public string Id => Email;

        public string UserName
        {
            get { return Email; }
            set { Email = value; }
        }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}