using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PollySamples.Controllers.DelegateOnRetryPolicySample
{
    [Route("api/samples/delegate-on-retry-policy/[controller]"), Produces("application/json")]
    public class CatalogController : Controller
    {
        readonly AsyncRetryPolicy<HttpResponseMessage> _httpRetryPolicy;

        HttpClient _httpClient;

        public CatalogController()
        {
            _httpRetryPolicy = Policy
                .HandleResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode)
                .RetryAsync(3, onRetry: (httpResponseMessage, i) =>
                {
                    if (httpResponseMessage.Result.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        PerformReauthorization();
                    }
                });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            _httpClient = BuildHttpClient("BadAuthCode");

            string requestEndpoint = $"samples/delegate-on-retry-policy/inventory/{id}";

            var response = await _httpRetryPolicy.ExecuteAsync(() => _httpClient.GetAsync(requestEndpoint));
            if (response.IsSuccessStatusCode)
            {
                var itemsInStock = JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());

                return Ok(itemsInStock);
            }

            return StatusCode((int)response.StatusCode, response.Content.ReadAsStringAsync());
        }

        private void PerformReauthorization()
        {
            _httpClient = BuildHttpClient("GoodAuthCode");
        }

        private HttpClient BuildHttpClient(string authCookieValue)
        {
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(new Uri("http://localhost"), new Cookie("Auth", authCookieValue));

            var httpClientHandler = new HttpClientHandler() { CookieContainer = cookieContainer };

            var httpClient = new HttpClient(httpClientHandler);
            httpClient.BaseAddress = new Uri(@"https://localhost:5001/api/");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return httpClient;
        }
    }
}
