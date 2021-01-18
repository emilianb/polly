using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly.Bulkhead;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace PollySamples.Controllers.BulkheadSample
{
    [Route("api/samples/bulkhead/[controller]"), Produces("application/json")]
    public class CatalogController : Controller
    {
        static int _requestCount = 0;

        readonly HttpClient _httpClient;

        readonly AsyncBulkheadPolicy<HttpResponseMessage> _bulkheadIsolationPolicy;

        public CatalogController(HttpClient httpClient, AsyncBulkheadPolicy<HttpResponseMessage> bulkheadIsolationPolicy)
        {
            _httpClient = httpClient;

            _bulkheadIsolationPolicy = bulkheadIsolationPolicy;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            _requestCount++;

            LogBulkheadInfo();

            string requestEndpoint = $"samples/bulkhead/inventory/{id}";

            var response = await _bulkheadIsolationPolicy.ExecuteAsync(
                    async () => await _httpClient.GetAsync(requestEndpoint));

            if (response.IsSuccessStatusCode)
            {
                var itemsInStock = JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());

                return Ok(itemsInStock);
            }

            if (response.Content != null)
            {
                return StatusCode((int)response.StatusCode, response.Content.ReadAsStringAsync());
            }

            return StatusCode((int)response.StatusCode);
        }

        private void LogBulkheadInfo()
        {
            Debug.WriteLine($"PollyDemo RequestCount {_requestCount}");
            Debug.WriteLine($"PollyDemo BulkheadAvailableCount " + $"{_bulkheadIsolationPolicy.BulkheadAvailableCount}");
            Debug.WriteLine($"PollyDemo QueueAvailableCount " + $"{_bulkheadIsolationPolicy.QueueAvailableCount}");
        }
    }
}
