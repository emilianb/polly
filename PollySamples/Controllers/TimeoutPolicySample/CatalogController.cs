using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly;
using Polly.Fallback;
using Polly.Retry;
using Polly.Timeout;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace PollySamples.Controllers.TimeoutPolicySample
{
    [Route("api/samples/timeout-policy/[controller]"), Produces("application/json")]
    public class CatalogController : Controller
    {
        readonly AsyncTimeoutPolicy _httpTimeoutPolicy;
        readonly AsyncRetryPolicy<HttpResponseMessage> _httpRetryPolicy;
        readonly AsyncFallbackPolicy<HttpResponseMessage> _httpFallbackPolicy;

        int _cachedResult = 0;

        public CatalogController()
        {
            // throws TimeoutRejectedException if timeout of 1 second is exceeded.
            _httpTimeoutPolicy = Policy.TimeoutAsync(1);

            _httpRetryPolicy = Policy
                .HandleResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode)
                .Or<TimeoutRejectedException>()
                .RetryAsync(3);

            _httpFallbackPolicy = Policy
                .HandleResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode)
                .Or<TimeoutRejectedException>()
                .FallbackAsync(
                    new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ObjectContent(_cachedResult.GetType(), _cachedResult, new JsonMediaTypeFormatter())
                    }
                );
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var httpClient = GetHttpClient();

            string requestEndpoint = $"samples/timeout-policy/inventory/{id}";

            var response = await _httpFallbackPolicy
                .ExecuteAsync(() => _httpRetryPolicy
                    .ExecuteAsync(() => _httpTimeoutPolicy
                        .ExecuteAsync(token => httpClient.GetAsync(requestEndpoint, token), CancellationToken.None)));

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

        private HttpClient GetHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(@"https://localhost:5001/api/");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return httpClient;
        }
    }
}
