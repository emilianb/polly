using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace PollySamples.Controllers.FallbackPolicySample
{
    [Route("api/samples/fallback-policy/[controller]"), Produces("application/json")]
    public class InventoryController : Controller
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            // Simulate some data processing by delaying for 100 milliseconds.
            await Task.Delay(100);

            return StatusCode((int)HttpStatusCode.InternalServerError, "Something went wrong!");
        }
    }
}
