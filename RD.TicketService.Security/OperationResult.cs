namespace RD.TicketService.Security
{
    public class OperationResult
    {
        public static readonly OperationResult Success = new OperationResult { Succeeded = true };

        public bool Succeeded { get; set; } 
        public string[] Errors { get; set; }
    }
}