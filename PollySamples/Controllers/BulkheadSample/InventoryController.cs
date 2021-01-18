using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace PollySamples.Controllers.BulkheadSample
{
    [Route("api/samples/bulkhead/[controller]"), Produces("application/json")]
    public class InventoryController : Controller
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            // simulate some data processing by delaying for 10 seconds 
            await Task.Delay(10000);

            return Ok(15);
        }
    }
}
