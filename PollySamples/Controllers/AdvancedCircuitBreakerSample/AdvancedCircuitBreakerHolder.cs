using Polly.CircuitBreaker;
using System.Net.Http;

namespace PollySamples.Controllers.AdvancedCircuitBreakerSample
{
    public class AdvancedCircuitBreakerHolder
    {
        public AsyncCircuitBreakerPolicy<HttpResponseMessage> BreakerPolicy{ get; set; }
    }
}
