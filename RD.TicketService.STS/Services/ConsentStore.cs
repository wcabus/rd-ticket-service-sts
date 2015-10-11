using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using RD.TicketService.Security;
using Consent = IdentityServer3.Core.Models.Consent;

namespace RD.TicketService.STS.Services
{
    public class ConsentStore<TUser, TId> : IConsentStore
        where TUser : class, IUser<TId>, new()
        where TId : IEquatable<TId>
    {
        private readonly IUserStore<TUser, TId> _userStore;

        public ConsentStore(IUserStore<TUser, TId> userStore)
        {
            _userStore = userStore;
        }

        private Consent ConsentConverter(Security.Consent consent)
        {
            if (consent == null)
            {
                return null;
            }

            return new Consent
            {
                ClientId = consent.Client,
                Scopes = consent.ScopeList.Split(' ').AsEnumerable(),
                Subject = consent.Subject
            };
        }

        public async Task<IEnumerable<Consent>> LoadAllAsync(string subject)
        {
            return (await _userStore.FindConsentByNameAsync(subject)).Select(ConsentConverter);
        }

        public async Task RevokeAsync(string subject, string client)
        {
            await _userStore.RevokeConsentAsync(subject, client);
        }

        public async Task<Consent> LoadAsync(string subject, string client)
        {
            return ConsentConverter(await _userStore.FindConsentByNameAndClientAsync(subject, client));
        }

        public async Task UpdateAsync(Consent consent)
        {
            await _userStore.CreateOrUpdateConsentAsync(consent.ClientId, consent.Subject, string.Join(" ", consent.Scopes));
        }
    }
}