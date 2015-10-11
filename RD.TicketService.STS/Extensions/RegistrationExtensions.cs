using IdentityServer3.Core.Services;
using RD.TicketService.Security;
using RD.TicketService.Security.Samples;
using RD.TicketService.STS.Services;

// ReSharper disable once CheckNamespace
namespace IdentityServer3.Core.Configuration
{
    public static class RegistrationExtensions
    {
        public static void ConfigureUserService(this IdentityServerServiceFactory factory)
        {
            factory.UserService = new Registration<IUserService, UserService<User, string>>();
            factory.Register(new Registration<IUserStore<User, string>>(new InMemoryUserStore()));
        }

        public static void ConfigureConsentService(this IdentityServerServiceFactory factory)
        {
            factory.ConsentStore = new Registration<IConsentStore, ConsentStore<User, string>>();
        }
    }
}