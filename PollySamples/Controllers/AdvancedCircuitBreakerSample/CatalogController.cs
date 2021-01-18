using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Net.Http;
using System.Threading.Tasks;

namespace PollySamples.Controllers.CircuitBreakerSample
{
    [Route("api/samples/circuit-breaker/[controller]"), Produces("application/json")]
    public class CatalogController : Controller
    {
        readonly HttpClient _httpClient;

        readonly AsyncRetryPolicy<HttpResponseMessage> _httpRetryPolicy;

        readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _breakerPolicy;

        public CatalogController(HttpClient httpClient, AsyncCircuitBreakerPolicy<HttpResponseMessage> breakerPolicy)
        {
            _httpClient = httpClient;

            _breakerPolicy = breakerPolicy;

            _httpRetryPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode).RetryAsync(2);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            string requestEndpoint = $"samples/circuit-breaker/inventory/{id}";

            var response = await _httpRetryPolicy.ExecuteAsync(
                () => _breakerPolicy.ExecuteAsync(
                    async () => await _httpClient.GetAsync(requestEndpoint)));

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

        [HttpGet("pricing/{id}")]
        public async Task<IActionResult> GetPricing(int id)
        {
            string requestEndpoint = $"samples/circuit-breaker/pricing/{id}";

            var response = await _httpRetryPolicy.ExecuteAsync(
                () => _breakerPolicy.ExecuteAsync(
                    () => _httpClient.GetAsync(requestEndpoint)));

            if (response.IsSuccessStatusCode)
            {
                var priceOfItem = JsonConvert.DeserializeObject<decimal>(await response.Content.ReadAsStringAsync());

                return Ok($"${priceOfItem}");
            }

            return StatusCode((int)response.StatusCode, response.Content.ReadAsStringAsync());
        }
    }
}
