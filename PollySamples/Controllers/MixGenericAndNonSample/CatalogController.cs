using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly;
using Polly.Fallback;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace PollySamples.Controllers.MixGenericAndNonSample
{
    [Route("api/samples/mix-generic-and-non/[controller]"), Produces("application/json")]
    public class CatalogController : Controller
    {
        readonly int _cachedResult = 0;

        readonly AsyncTimeoutPolicy<HttpResponseMessage> _timeoutPolicy;

        readonly AsyncRetryPolicy<HttpResponseMessage> _httpRetryPolicy;

        readonly AsyncFallbackPolicy<HttpResponseMessage> _httpRequestFallbackPolicy;

        readonly AsyncPolicyWrap<HttpResponseMessage> _policyWrap;

        public CatalogController()
        {
            _timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(1, onTimeoutAsync: TimeoutDelegate);

            _httpRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .Or<TimeoutRejectedException>()
                    .RetryAsync(3, onRetry: HttpRetryPolicyDelegate);

            _httpRequestFallbackPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<TimeoutRejectedException>()
                .FallbackAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ObjectContent(_cachedResult.GetType(), _cachedResult, new JsonMediaTypeFormatter())
                }, onFallbackAsync: HttpRequestFallbackPolicyDelegate);

            _policyWrap = Policy.WrapAsync(_httpRequestFallbackPolicy, _httpRetryPolicy, _timeoutPolicy);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var httpClient = GetHttpClient();

            string requestEndpoint = $"samples/mix-generic-and-non/inventory/{id}";

            var response = await _policyWrap.ExecuteAsync(token => httpClient.GetAsync(requestEndpoint, token), CancellationToken.None);

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

        private Task TimeoutDelegate(Context context, TimeSpan timeSpan, Task arg3)
        {
            Debug.WriteLine("In OnTimeoutAsync");

            return Task.CompletedTask;
        }

        private void HttpRetryPolicyDelegate(DelegateResult<HttpResponseMessage> delegateResult, int i)
        {
            Debug.WriteLine("In HttpRetryPolicyDelegate");
        }

        private Task HttpRequestFallbackPolicyDelegate(DelegateResult<HttpResponseMessage> delegateResult, Context context)
        {
            Debug.WriteLine("In OnFallbackAsync");

            return Task.CompletedTask;
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
