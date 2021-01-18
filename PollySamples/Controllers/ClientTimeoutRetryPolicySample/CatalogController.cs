using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PollySamples.Controllers.ClientTimeoutRetryPolicySample
{
    [Route("api/samples/client-timeout-retry-policy/[controller]"), Produces("application/json")]
    public class CatalogController : Controller
    {
        readonly AsyncRetryPolicy<HttpResponseMessage> _httpRetryPolicy;

        public CatalogController()
        {
            _httpRetryPolicy = Policy
                .HandleResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode)
                .Or<HttpRequestException>()
                .RetryAsync(3, onRetry: OnRetry);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var httpClient = GetHttpClient();

            string requestEndpoint = $"samples/client-timeout-retry-policy/inventory/{id}";

            var response = await _httpRetryPolicy.ExecuteAsync(() => httpClient.GetAsync(requestEndpoint));
            if (response.IsSuccessStatusCode)
            {
                var itemsInStock = JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());

                return Ok(itemsInStock);
            }

            return StatusCode((int)response.StatusCode, response.Content.ReadAsStringAsync());
        }

        private void OnRetry(DelegateResult<HttpResponseMessage> delegateResult, int retryCount)
        {
            if (delegateResult.Exception is HttpRequestException)
            {
                // if (delegateResult.Exception.GetBaseException().Message == "The operation timed out")
                if (delegateResult.Exception.GetBaseException().Message == "No connection could be made because the target machine actively refused it.")
                {
                    // log something about the timeout
                    Console.WriteLine("The operation timed out");
                }
            }
        }

        private HttpClient GetHttpClient()
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(@"https://localhost:5555/api/");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return httpClient;
        }
    }
}
