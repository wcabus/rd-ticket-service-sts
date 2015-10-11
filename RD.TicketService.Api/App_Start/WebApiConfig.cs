using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Cors;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RD.TicketService.Api.Filters;

namespace RD.TicketService.Api
{
    public static class WebApiConfig
    {
        public static HttpConfiguration Register()
        {
            var config = new HttpConfiguration();

            // CORS
            config.EnableCors(new EnableCorsAttribute("*","*","*"));

            // Only support JSON media type formatting using camelCase.
            // And we disable reference loop handling, to prevent issues with Customer => Orders => Customer => Orders...
            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter
            {
                SerializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }
            });

            // Add global filters
            config.Filters.Add(new RequireHttpsAttribute());
            config.Filters.Add(new AuthorizeAttribute());

            // Web API routes
            config.MapHttpAttributeRoutes();

            return config;
        }
    }
}