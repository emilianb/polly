using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace PollySamples.Controllers.SharePoliciesByRegistrySample
{
    [Route("api/samples/share-policies-by-registry/[controller]"), Produces("application/json")]
    public class InventoryController : Controller
    {
        static int _requestCount = 0;

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            // Simulate some data processing by delaying for 100 milliseconds.
            await Task.Delay(100);

            _requestCount++;

            // only one of out four requests will succeed
            if (_requestCount % 4 == 0)
            {
                return Ok(15);
            }

            return StatusCode((int)HttpStatusCode.InternalServerError, "Something went wrong!");
        }
    }
}
