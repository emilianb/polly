using Polly;
using Polly.Wrap;
using System.Net.Http;

namespace PollySamples.Controllers.WrappingReuseSample
{
    public interface IPolicyHolder
    {
        IAsyncPolicy<HttpResponseMessage> TimeoutPolicy { get; set; }

        IAsyncPolicy<HttpResponseMessage> HttpRetryPolicy { get; set; }

        IAsyncPolicy<HttpResponseMessage> HttpRequestFallbackPolicy { get; set; }

        AsyncPolicyWrap<HttpResponseMessage> TimeoutRetryAndFallbackWrap { get; set; }
    }
}
