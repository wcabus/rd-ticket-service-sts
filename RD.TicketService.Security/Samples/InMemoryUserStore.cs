using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RD.TicketService.Security.Samples
{
    public class InMemoryUserStore : IUserStore<User, string>
    {
        private readonly List<User> _users = new List<User>();
        private readonly IDictionary<string, IEnumerable<string>> _roleDictionary = new Dictionary<string, IEnumerable<string>>();
        private readonly IDictionary<string, IList<Claim>> _claimDictionary = new Dictionary<string, IList<Claim>>();
        private readonly IDictionary<string, IList<ExternalLoginInfo>> _externalLoginDictionary = new Dictionary<string, IList<ExternalLoginInfo>>();
        private readonly IDictionary<string, IList<Consent>> _consentDictionary = new Dictionary<string, IList<Consent>>(); 

        public InMemoryUserStore()
        {
            _users.Add(new User
            {
                Email = "wesley.cabus@realdolmen.com",
                Password = "test",
                FirstName = "Wesley",
                LastName = "Cabus"
            });
        }

        public Task<OperationResult> CreateAsync(User user)
        {
            if (_users.Any(u => string.Equals(u.UserName, user.UserName, StringComparison.OrdinalIgnoreCase)))
            {
                return Task.FromResult(new OperationResult {Succeeded = false, Errors = new[] {"Duplicate username"}});
            }

            _users.Add(user);
            return Task.FromResult(OperationResult.Success);
        }

        public Task<OperationResult> AddLoginAsync(string userId, ExternalLoginInfo loginInfo)
        {
            IList<ExternalLoginInfo> externalLogins;
            if (_externalLoginDictionary.TryGetValue(userId, out externalLogins))
            {
                if (
                    externalLogins.Any(
                        e => string.Equals(e.Provider, loginInfo.Provider, StringComparison.OrdinalIgnoreCase)))
                {
                    return Task.FromResult(new OperationResult
                    {
                        Succeeded = false,
                        Errors = new [] { "Duplicate external login" }
                    });
                }

                externalLogins.Add(loginInfo);
            }
            else
            {
                _externalLoginDictionary.Add(userId, new List<ExternalLoginInfo> { loginInfo });
            }

            return Task.FromResult(OperationResult.Success);
        }

        public Task<User> FindByIdAsync(string id)
        {
            return
                Task.FromResult(_users.FirstOrDefault(u => string.Equals(u.Id, id, StringComparison.OrdinalIgnoreCase)));
        }

        public Task<User> FindByNameAsync(string username)
        {
            return
                Task.FromResult(_users.FirstOrDefault(u => string.Equals(u.UserName, username, StringComparison.OrdinalIgnoreCase)));
        }

        public Task<User> FindAsync(ExternalLoginInfo externalLoginInfo)
        {
            foreach (var pair in _externalLoginDictionary)
            {
                if (pair.Value != null &&
                    pair.Value.Any(
                        e =>
                            e.Provider == externalLoginInfo.Provider &&
                            e.NameIdentifier == externalLoginInfo.NameIdentifier))
                {
                    return FindByIdAsync(pair.Key);
                }
            }

            return Task.FromResult<User>(null);
        }

        public bool SupportsUserClaim => true;

        public Task<IEnumerable<Claim>> GetClaimsAsync(string userId)
        {
            IList<Claim> claims;
            var user = FindByIdAsync(userId).Result;
            if (!_claimDictionary.TryGetValue(userId, out claims))
            {
                claims = new List<Claim>();
            }

            claims.Add(new Claim("given_name", user.FirstName));
            claims.Add(new Claim("family_name", user.LastName));

            return Task.FromResult<IEnumerable<Claim>>(claims);
        }

        public Task<OperationResult> AddClaimAsync(string userId, Claim claim)
        {
            IList<Claim> claims;
            if (_claimDictionary.TryGetValue(userId, out claims))
            {
                claims.Add(claim);
            }
            else
            {
                _claimDictionary.Add(userId, new List<Claim> { claim });
            }

            return Task.FromResult(OperationResult.Success);
        }

        public bool SupportsUserRole => true;
        public Task<IEnumerable<string>> GetRolesAsync(string userId)
        {
            IEnumerable<string> roles;
            if (_roleDictionary.TryGetValue(userId, out roles))
            {
                return Task.FromResult(roles);
            }

            return Task.FromResult<IEnumerable<string>>(new List<string>());
        }

        public bool SupportsUserPassword => true;
        public Task<bool> CheckPasswordAsync(User user, string password)
        {
            return Task.FromResult(user.Password == password);
        }

        public bool SupportsUserLockout => false;
        public Task<bool> IsLockedOutAsync(string userId)
        {
            throw new System.NotImplementedException();
        }

        public Task ResetAccessFailedCountAsync(string userId)
        {
            throw new System.NotImplementedException();
        }

        public Task AccessFailedAsync(string userId)
        {
            throw new System.NotImplementedException();
        }

        public bool SupportsUserEmail => true;
        public Task<string> GetEmailAsync(string userId)
        {
            var user = FindByIdAsync(userId).Result;
            if (user == null)
            {
                return Task.FromResult<string>(null);
            }

            return Task.FromResult(user.Email);
        }

        public Task<OperationResult> SetEmailAsync(string userId, string email)
        {
            var user = FindByIdAsync(userId).Result;
            if (user == null)
            {
                return Task.FromResult(new OperationResult
                {
                    Succeeded = false,
                    Errors = new [] { "Unknown user" }
                });
            }

            user.Email = email;
            return Task.FromResult(OperationResult.Success);
        }

        public bool SupportsUserSecurityStamp => false;
        public Task<string> GetSecurityStampAsync(string id)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<Consent>> FindConsentByNameAsync(string userName)
        {
            IList<Consent> consents;
            if (_consentDictionary.TryGetValue(userName, out consents))
            {
                return Task.FromResult<IEnumerable<Consent>>(consents);
            }

            return Task.FromResult<IEnumerable<Consent>>(new List<Consent>());
        }

        public Task<Consent> FindConsentByNameAndClientAsync(string userName, string client)
        {
            IList<Consent> consents;
            if (_consentDictionary.TryGetValue(userName, out consents))
            {
                return Task.FromResult(consents.FirstOrDefault(c => c.Client == client));
            }

            return Task.FromResult<Consent>(null);
        }

        public Task CreateOrUpdateConsentAsync(string clientId, string userName, string scopeList)
        {
            IList<Consent> consents;
            if (!_consentDictionary.TryGetValue(userName, out consents))
            {
                _consentDictionary.Add(userName, new List<Consent>());
                consents = _consentDictionary[userName];
            }

            var consent = consents.FirstOrDefault(c => c.Client == clientId && c.Subject == userName);
            if (consent == null)
            {
                consent = new Consent
                {
                    Client = clientId,
                    Subject = userName,
                    ScopeList = scopeList
                };
                consents.Add(consent);
            }
            else
            {
                consent.ScopeList = scopeList;
            }

            return Task.FromResult(0);
        }

        public Task RevokeConsentAsync(string userName, string client)
        {
            IList<Consent> consents;
            if (_consentDictionary.TryGetValue(userName, out consents))
            {
                var consent = consents.FirstOrDefault(c => c.Client == client && c.Subject == userName);
                if (consent != null)
                {
                    consents.Remove(consent);
                }
            }

            return Task.FromResult(0);
        }
    }
}