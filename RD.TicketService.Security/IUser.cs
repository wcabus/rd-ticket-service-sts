using System;

namespace RD.TicketService.Security
{
    /// <summary>
    /// Defines a contract for retrieving identity information from a user.
    /// </summary>
    /// <typeparam name="TId">The type of ID</typeparam>
    public interface IUser<out TId> where TId : IEquatable<TId>
    {
        TId Id { get; }

        string UserName { get; set; }
    }
}