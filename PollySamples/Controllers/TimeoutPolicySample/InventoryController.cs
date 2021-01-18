using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace PollySamples.Controllers.TimeoutPolicySample
{
    [Route("api/samples/timeout-policy/[controller]"), Produces("application/json")]
    public class InventoryController : Controller
    {
        static int _requestCount = 0;

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            _requestCount++;

            if (_requestCount % 6 != 0)
            {
                // Simulate some data processing by delaying for 10 seconds.
                await Task.Delay(10000);
            }

            return Ok(15);
        }
    }
}
