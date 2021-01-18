using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Net.Http;
using System.Threading.Tasks;

namespace PollySamples.Controllers.AdvancedCircuitBreakerSample
{
    [Route("api/samples/advanced-circuit-breaker/[controller]"), Produces("application/json")]
    public class CatalogController : Controller
    {
        readonly HttpClient _httpClient;

        readonly AsyncRetryPolicy<HttpResponseMessage> _httpRetryPolicy;

        readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _breakerPolicy;

        public CatalogController(HttpClient httpClient, AdvancedCircuitBreakerHolder breakerPolicyHolder)
        {
            _httpClient = httpClient;

            _breakerPolicy = breakerPolicyHolder.BreakerPolicy;

            _httpRetryPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode).RetryAsync(2);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            string requestEndpoint = $"samples/advanced-circuit-breaker/inventory/{id}";

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
    }
}
