using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace PollySamples.Controllers.CircuitBreakerSample
{
    [Route("api/samples/circuit-breaker/[controller]"), Produces("application/json")]
    public class PricingController : Controller
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            await Task.Delay(100); // simulate some data processing by delaying for 100 milliseconds 

            return Ok(id + 10.27);
        }
    }
}
