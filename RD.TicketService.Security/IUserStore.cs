using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RD.TicketService.Security
{
    public interface IUserStore<TUser, in TId> 
        where TUser : IUser<TId>
        where TId : IEquatable<TId>
    {
        /// <summary>
        /// Creates the user in the data store
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<OperationResult> CreateAsync(TUser user);

        /// <summary>
        /// Adds the external login information to the user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="loginInfo"/>
        /// <returns></returns>
        Task<OperationResult> AddLoginAsync(TId userId, ExternalLoginInfo loginInfo);

        /// <summary>
        /// Finds the user using his or her id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TUser> FindByIdAsync(TId id);

        /// <summary>
        /// Finds the user using his or her username.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        Task<TUser> FindByNameAsync(string username);

        /// <summary>
        /// Finds the user using his or her external login (google, facebook, ...)
        /// </summary>
        /// <param name="externalLoginInfo"></param>
        /// <returns></returns>
        Task<TUser> FindAsync(ExternalLoginInfo externalLoginInfo);

        /// <summary>
        /// When implemented, returns <c>true</c> if we support claims.
        /// </summary>
        bool SupportsUserClaim{ get; }

        /// <summary>
        /// Returns the claims we know about the user identified by <paramref name="userId"/>
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <remarks>
        ///     Use this method to add additional claims to the users identity, like
        ///         - Constants.ClaimTypes.GivenName
        ///         - Constants.ClaimTypes.FamilyName
        /// </remarks>
        Task<IEnumerable<Claim>> GetClaimsAsync(TId userId);

        /// <summary>
        /// Add the given claim to the user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="claim"></param>
        /// <returns></returns>
        Task<OperationResult> AddClaimAsync(TId userId, Claim claim);

        /// <summary>
        /// When implemented, returns <c>true</c> if we support roles.
        /// </summary>
        bool SupportsUserRole { get; }

        /// <summary>
        /// Retrieves the roles for the user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<IEnumerable<string>> GetRolesAsync(TId userId);

        /// <summary>
        /// When implemented, returns <c>true</c> if we support authenticating a user using a password.
        /// </summary>
        bool SupportsUserPassword { get; }

        /// <summary>
        /// Returns <c>true</c> if the given password matches for the user.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<bool> CheckPasswordAsync(TUser user, string password);

        /// <summary>
        /// When implemented, returns <c>true</c> if we support locking users out of the application.
        /// </summary>
        bool SupportsUserLockout { get; }

        /// <summary>
        /// Returns <c>true</c> if the user, identified by <paramref name="userId"/> has been locked out.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> IsLockedOutAsync(TId userId);

        /// <summary>
        /// Resets the "failed login attempts" count for the user identified by <paramref name="userId"/>.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task ResetAccessFailedCountAsync(TId userId);

        /// <summary>
        /// Increases the "failed login attempts" count for the user identified by <paramref name="userId"/>.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task AccessFailedAsync(TId userId);

        /// <summary>
        /// When implemented, returns <c>true</c> if we support retrieving/setting the email address.
        /// </summary>
        bool SupportsUserEmail { get; }

        /// <summary>
        /// Retrieves the email address for the user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<string> GetEmailAsync(TId userId);

        /// <summary>
        /// Changes the email address for the user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        Task<OperationResult> SetEmailAsync(TId userId, string email);

        /// <summary>
        /// When implemented, returns true if we support security stamps
        /// </summary>
        /// <remarks>
        /// A security stamp is a unique value (guid, i.e.) which changes every time something
        /// changes considering the security of a user:
        /// - password changed
        /// - external login add/removed
        /// - username (email, i.e.) changed
        /// </remarks>
        bool SupportsUserSecurityStamp { get; }

        /// <summary>
        /// Retrieves the security stamp for the user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<string> GetSecurityStampAsync(TId id);

        /// <summary>
        /// Returns a list of consents given by the user identified by his name.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task<IEnumerable<Consent>> FindConsentByNameAsync(string userName);

        /// <summary>
        /// Returns the consent, if it exists, given by the user for the client application.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        Task<Consent> FindConsentByNameAndClientAsync(string userName, string client);

        /// <summary>
        /// Creates or updates the consent for the user and the client application
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="userName"></param>
        /// <param name="scopeList"></param>
        /// <returns></returns>
        Task CreateOrUpdateConsentAsync(string clientId, string userName, string scopeList);

        /// <summary>
        /// Revokes the consent given to the client application for the user.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        Task RevokeConsentAsync(string userName, string client);
    }
}