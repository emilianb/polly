using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace PollySamples.Controllers.DelegateOnRetryPolicySample
{
    [Route("api/samples/delegate-on-retry-policy/[controller]"), Produces("application/json")]
    public class InventoryController : Controller
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            // Simulate some data processing by delaying for 100 milliseconds.
            await Task.Delay(100);

            string authCode = Request.Cookies["Auth"];

            if (authCode == "GoodAuthCode")
            {
                return Ok(15);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.Unauthorized, "Not authorized");
            }
        }
    }
}
