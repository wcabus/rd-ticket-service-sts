using System;

namespace RD.TicketService.Security
{
    /// <summary>
    /// Defines a contract for retrieving identity information from a user.
    /// </summary>
    /// <typeparam name="TId">The type of ID</typeparam>
    public interface IUser<out TId> where TId : IEquatable<TId>
    {
        /// <summary>
        /// The identifier (subject) for users. This could be a GUID, a unique name or integer (identity in SQL) value
        /// </summary>
        TId Id { get; }

        /// <summary>
        /// The unique user name for a single user. This is being used when a user logs on locally.
        /// </summary>
        /// <remarks>
        /// Username could be the users e-mail address, for example.
        /// Local logon: when a user enters his username and password in our STS.
        /// External login: when a user uses his Google/Facebook/... account to log on.
        /// </remarks>
        string UserName { get; set; }
    }
}
