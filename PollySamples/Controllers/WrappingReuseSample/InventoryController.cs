﻿using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace PollySamples.Controllers.WrappingReuseSample
{
    [Route("api/samples/wrapping-reuse/[controller]"), Produces("application/json")]
    public class InventoryController : Controller
    {
        static int _requestCount = 0;

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            _requestCount++;

            if (_requestCount % 6 != 0)
            {
                // simulate some data processing by delaying for 10 seconds.
                await Task.Delay(10000);
            }

            return Ok(15);
        }
    }
}
