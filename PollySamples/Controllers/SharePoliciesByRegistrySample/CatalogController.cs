using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly;
using Polly.Registry;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PollySamples.Controllers.SharePoliciesByRegistrySample
{
    [Route("api/samples/share-policies-by-registry/[controller]"), Produces("application/json")]
    public class CatalogController : Controller
    {
        PolicyRegistry _policyRegistry;

        public CatalogController(PolicyRegistry policyRegistry)
        {
            _policyRegistry = policyRegistry;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var httpClient = GetHttpClient();

            string requestEndpoint = $"samples/share-policies-by-registry/inventory/{id}";

            var httpRetryPolicy = _policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>("SimpleHttpWaitAndRetry");

            var response = await httpRetryPolicy.ExecuteAsync(() => httpClient.GetAsync(requestEndpoint));
            if (response.IsSuccessStatusCode)
            {
                var itemsInStock = JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());

                return Ok(itemsInStock);
            }

            return StatusCode((int)response.StatusCode, response.Content.ReadAsStringAsync());
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
