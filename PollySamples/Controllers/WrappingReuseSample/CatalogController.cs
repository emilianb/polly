using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PollySamples.Controllers.WrappingReuseSample
{
    [Route("api/samples/wrapping-reuse/[controller]"), Produces("application/json")]
    public class CatalogController : Controller
    {
        readonly HttpClient _httpClient;

        readonly IPolicyHolder _policyHolder;

        public CatalogController(IPolicyHolder policyHolder, HttpClient httpClient)
        {
            _policyHolder = policyHolder;
            _httpClient = httpClient;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            string requestEndpoint = $"samples/wrapping-reuse/inventory/{id}";

            var response = await _policyHolder
                .TimeoutRetryAndFallbackWrap.ExecuteAsync(
                    async token => await _httpClient.GetAsync(requestEndpoint, token),
                    CancellationToken.None);

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
