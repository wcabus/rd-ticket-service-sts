using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace RD.TicketService.Api.Controllers
{
    public class TestController : ApiController
    {
        [Route("api/customers")]
        public IHttpActionResult GetCustomers()
        {
            return Ok(new[] {new {Id = 1, Name = "Wesley Cabus", Email = "wesley.cabus@realdolmen.com"}});
        }

        [Route("api/products")]
        [AllowAnonymous]
        public IHttpActionResult GetProducts()
        {
            return Ok(new[] { new { Id = 1, Name = "Chimichangas", Price = 9.99m } });
        }

        [Route("claims")]
        public IHttpActionResult GetMyClaims()
        {
            var claims = ((ClaimsPrincipal) User).Claims;
            return Ok(claims.Select(x => new {x.Type, x.Value}));
        }
    }
}
