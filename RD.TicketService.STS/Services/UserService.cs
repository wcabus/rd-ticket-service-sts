using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core;
using IdentityServer3.Core.Services.Default;
using RD.TicketService.Security;

namespace RD.TicketService.STS.Services
{
    public class UserService<TUser, TId> : UserServiceBase
        where TUser : class, IUser<TId>, new()
        where TId : IEquatable<TId>
    {
        private readonly IUserStore<TUser, TId> _userStore;
        private readonly Func<string, TId> _convertSubjectToKey;

        public UserService(IUserStore<TUser, TId> userStore, Func<string, TId> parseSubject = null)
        {
            if (userStore == null) throw new ArgumentNullException(nameof(userStore));

            _userStore = userStore;

            if (parseSubject != null)
            {
                _convertSubjectToKey = parseSubject;
            }
            else
            {
                var keyType = typeof(TId);
                if (keyType == typeof(string)) _convertSubjectToKey = subject => (TId)ParseString(subject);
                else if (keyType == typeof(int)) _convertSubjectToKey = subject => (TId)ParseInt(subject);
                else if (keyType == typeof(uint)) _convertSubjectToKey = subject => (TId)ParseUInt32(subject);
                else if (keyType == typeof(long)) _convertSubjectToKey = subject => (TId)ParseLong(subject);
                else if (keyType == typeof(Guid)) _convertSubjectToKey = subject => (TId)ParseGuid(subject);
                else
                {
                    throw new InvalidOperationException("Key type not supported");
                }
            }

            EnableSecurityStamp = true;
        }

        public string DisplayNameClaimType { get; set; }
        public bool EnableSecurityStamp { get; set; }

        private object ParseString(string sub)
        {
            return sub;
        }
        private object ParseInt(string sub)
        {
            int key;
            if (!int.TryParse(sub, out key)) return 0;
            return key;
        }

        private object ParseUInt32(string sub)
        {
            uint key;
            if (!uint.TryParse(sub, out key)) return 0;
            return key;
        }

        private object ParseLong(string sub)
        {
            long key;
            if (!long.TryParse(sub, out key)) return 0;
            return key;
        }

        private object ParseGuid(string sub)
        {
            Guid key;
            if (!Guid.TryParse(sub, out key)) return Guid.Empty;
            return key;
        }

        public override async Task GetProfileDataAsync(ProfileDataRequestContext ctx)
        {
            var subject = ctx.Subject;
            var requestedClaimTypes = ctx.RequestedClaimTypes;

            if (subject == null) throw new ArgumentNullException("subject");

            TId id = _convertSubjectToKey(subject.GetSubjectId());
            var acct = await _userStore.FindByIdAsync(id);
            if (acct == null)
            {
                throw new ArgumentException("Invalid subject identifier");
            }

            var claims = await GetClaimsFromAccount(acct);
            if (requestedClaimTypes != null && requestedClaimTypes.Any())
            {
                claims = claims.Where(x => requestedClaimTypes.Contains(x.Type));
            }

            ctx.IssuedClaims = claims;
        }

        protected virtual async Task<IEnumerable<Claim>> GetClaimsFromAccount(TUser user)
        {
            var claims = new List<Claim>{
                new Claim(Constants.ClaimTypes.Subject, user.Id.ToString()),
                new Claim(Constants.ClaimTypes.PreferredUserName, user.UserName)
            };

            if (_userStore.SupportsUserEmail)
            {
                var email = await _userStore.GetEmailAsync(user.Id);
                if (!string.IsNullOrWhiteSpace(email))
                {
                    claims.Add(new Claim(Constants.ClaimTypes.Email, email));
                }
            }
            
            if (_userStore.SupportsUserClaim)
            {
                claims.AddRange(await _userStore.GetClaimsAsync(user.Id));
            }

            if (_userStore.SupportsUserRole)
            {
                var roleClaims =
                    from role in await _userStore.GetRolesAsync(user.Id)
                    select new Claim(Constants.ClaimTypes.Role, role);
                claims.AddRange(roleClaims);
            }

            return claims;
        }

        protected virtual async Task<string> GetDisplayNameForAccountAsync(TId userId)
        {
            var user = await _userStore.FindByIdAsync(userId);
            var claims = await GetClaimsFromAccount(user);

            Claim nameClaim = null;
            if (DisplayNameClaimType != null)
            {
                nameClaim = claims.FirstOrDefault(x => x.Type == DisplayNameClaimType);
            }
            if (nameClaim == null) nameClaim = claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.Name);
            if (nameClaim == null) nameClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
            if (nameClaim != null) return nameClaim.Value;

            return user.UserName;
        }

        protected virtual async Task<TUser> FindUserAsync(string username)
        {
            return await _userStore.FindByNameAsync(username);
        }

        protected virtual Task<AuthenticateResult> PostAuthenticateLocalAsync(TUser user, SignInMessage message)
        {
            return Task.FromResult<AuthenticateResult>(null);
        }

        public override async Task AuthenticateLocalAsync(LocalAuthenticationContext ctx)
        {
            var username = ctx.UserName;
            var password = ctx.Password;
            var message = ctx.SignInMessage;

            ctx.AuthenticateResult = null;

            if (_userStore.SupportsUserPassword)
            {
                var user = await FindUserAsync(username);
                if (user != null)
                {
                    if (_userStore.SupportsUserLockout && await _userStore.IsLockedOutAsync(user.Id))
                    {
                        return;
                    }

                    if (await _userStore.CheckPasswordAsync(user, password))
                    {
                        if (_userStore.SupportsUserLockout)
                        {
                            await _userStore.ResetAccessFailedCountAsync(user.Id);
                        }

                        var result = await PostAuthenticateLocalAsync(user, message);
                        if (result == null)
                        {
                            var claims = await GetClaimsForAuthenticateResult(user);
                            result = new AuthenticateResult(user.Id.ToString(), await GetDisplayNameForAccountAsync(user.Id), claims);
                        }

                        ctx.AuthenticateResult = result;
                    }
                    else if (_userStore.SupportsUserLockout)
                    {
                        await _userStore.AccessFailedAsync(user.Id);
                    }
                }
            }
        }
        protected virtual async Task<IEnumerable<Claim>> GetClaimsForAuthenticateResult(TUser user)
        {
            List<Claim> claims = new List<Claim>();
            if (EnableSecurityStamp && _userStore.SupportsUserSecurityStamp)
            {
                var stamp = await _userStore.GetSecurityStampAsync(user.Id);
                if (!string.IsNullOrWhiteSpace(stamp))
                {
                    claims.Add(new Claim("security_stamp", stamp));
                }
            }
            return claims;
        }

        public override async Task AuthenticateExternalAsync(ExternalAuthenticationContext ctx)
        {
            var externalUser = ctx.ExternalIdentity;
            if (externalUser == null)
            {
                throw new ArgumentNullException("externalUser");
            }

            var user = await _userStore.FindAsync(new ExternalLoginInfo(externalUser.Provider, externalUser.ProviderId));
            if (user == null)
            {
                ctx.AuthenticateResult = await ProcessNewExternalAccountAsync(externalUser.Provider, externalUser.ProviderId, externalUser.Claims);
            }
            else
            {
                ctx.AuthenticateResult = await ProcessExistingExternalAccountAsync(user.Id, externalUser.Provider, externalUser.ProviderId, externalUser.Claims);
            }
        }

        protected virtual async Task<AuthenticateResult> ProcessNewExternalAccountAsync(string provider, string providerId, IEnumerable<Claim> claims)
        {
            var user = await TryGetExistingUserFromExternalProviderClaimsAsync(provider, claims);
            if (user == null)
            {
                user = await InstantiateNewUserFromExternalProviderAsync(provider, providerId, claims);
                if (user == null)
                    throw new InvalidOperationException("CreateNewAccountFromExternalProvider returned null");

                var createResult = await _userStore.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return new AuthenticateResult(createResult.Errors.First());
                }
            }

            var externalLogin = new ExternalLoginInfo(provider, providerId);
            var addExternalResult = await _userStore.AddLoginAsync(user.Id, externalLogin);
            if (!addExternalResult.Succeeded)
            {
                return new AuthenticateResult(addExternalResult.Errors.First());
            }

            var result = await AccountCreatedFromExternalProviderAsync(user.Id, provider, providerId, claims);
            if (result != null) return result;

            return await SignInFromExternalProviderAsync(user.Id, provider);
        }

        protected virtual Task<TUser> InstantiateNewUserFromExternalProviderAsync(string provider, string providerId, IEnumerable<Claim> claims)
        {
            var user = new TUser { UserName = Guid.NewGuid().ToString("N") };
            return Task.FromResult(user);
        }

        protected virtual Task<TUser> TryGetExistingUserFromExternalProviderClaimsAsync(string provider, IEnumerable<Claim> claims)
        {
            return Task.FromResult<TUser>(null);
        }

        protected virtual async Task<AuthenticateResult> AccountCreatedFromExternalProviderAsync(TId userId, string provider, string providerId, IEnumerable<Claim> claims)
        {
            claims = await SetAccountEmailAsync(userId, claims);
            return await UpdateAccountFromExternalClaimsAsync(userId, provider, providerId, claims);
        }

        protected virtual async Task<AuthenticateResult> SignInFromExternalProviderAsync(TId userId, string provider)
        {
            var user = await _userStore.FindByIdAsync(userId);
            var claims = await GetClaimsForAuthenticateResult(user);

            return new AuthenticateResult(
                userId.ToString(),
                await GetDisplayNameForAccountAsync(userId),
                claims,
                authenticationMethod: Constants.AuthenticationMethods.External,
                identityProvider: provider);
        }

        protected virtual async Task<AuthenticateResult> UpdateAccountFromExternalClaimsAsync(TId userId, string provider, string providerId, IEnumerable<Claim> claims)
        {
            var existingClaims = await _userStore.GetClaimsAsync(userId);
            var intersection = existingClaims.Intersect(claims, new ClaimComparer());
            var newClaims = claims.Except(intersection, new ClaimComparer());

            foreach (var claim in newClaims)
            {
                var result = await _userStore.AddClaimAsync(userId, claim);
                if (!result.Succeeded)
                {
                    return new AuthenticateResult(result.Errors.First());
                }
            }

            return null;
        }

        protected virtual async Task<AuthenticateResult> ProcessExistingExternalAccountAsync(TId userId, string provider, string providerId, IEnumerable<Claim> claims)
        {
            return await SignInFromExternalProviderAsync(userId, provider);
        }

        protected virtual async Task<IEnumerable<Claim>> SetAccountEmailAsync(TId userId, IEnumerable<Claim> claims)
        {
            var email = claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.Email);
            if (email != null)
            {
                var userEmail = await _userStore.GetEmailAsync(userId);
                if (userEmail == null)
                {
                    // if this fails, then presumably the email is already associated with another account
                    // so ignore the error and let the claim pass thru
                    var result = await _userStore.SetEmailAsync(userId, email.Value);
                    if (result.Succeeded)
                    {
                        var email_verified = claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.EmailVerified);
                        if (email_verified != null && email_verified.Value == "true")
                        {
                            //var token = await _userStore.GenerateEmailConfirmationTokenAsync(userID);
                            //await _userStore.ConfirmEmailAsync(userID, token);
                        }

                        var emailClaims = new [] { Constants.ClaimTypes.Email, Constants.ClaimTypes.EmailVerified };
                        return claims.Where(x => !emailClaims.Contains(x.Type));
                    }
                }
            }

            return claims;
        }

        public override async Task IsActiveAsync(IsActiveContext ctx)
        {
            var subject = ctx.Subject;

            if (subject == null) throw new ArgumentNullException("subject");

            var id = subject.GetSubjectId();
            TId key = _convertSubjectToKey(id);
            var acct = await _userStore.FindByIdAsync(key);

            ctx.IsActive = false;

            if (acct != null)
            {
                if (EnableSecurityStamp && _userStore.SupportsUserSecurityStamp)
                {
                    var securityStamp = subject.Claims.Where(x => x.Type == "security_stamp").Select(x => x.Value).SingleOrDefault();
                    if (securityStamp != null)
                    {
                        var dbSecurityStamp = await _userStore.GetSecurityStampAsync(key);
                        if (dbSecurityStamp != securityStamp)
                        {
                            return;
                        }
                    }
                }

                ctx.IsActive = true;
            }
        }
    }
}