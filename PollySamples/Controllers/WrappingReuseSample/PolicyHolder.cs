using Polly;
using Polly.Timeout;
using Polly.Wrap;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;

namespace PollySamples.Controllers.WrappingReuseSample
{
    public class PolicyHolder : IPolicyHolder
    {
        readonly int _cachedResult = 0;

        public IAsyncPolicy<HttpResponseMessage> TimeoutPolicy { get; set; }

        public IAsyncPolicy<HttpResponseMessage> HttpRetryPolicy { get; set; }

        public IAsyncPolicy<HttpResponseMessage> HttpRequestFallbackPolicy { get; set; }

        public AsyncPolicyWrap<HttpResponseMessage> TimeoutRetryAndFallbackWrap { get; set; }

        public PolicyHolder()
        {
            TimeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(1);

            HttpRetryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<TimeoutRejectedException>()
                .RetryAsync(3);

            HttpRequestFallbackPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<TimeoutRejectedException>()
                .FallbackAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ObjectContent(_cachedResult.GetType(), _cachedResult, new JsonMediaTypeFormatter())
                });

            TimeoutRetryAndFallbackWrap = Policy.WrapAsync(HttpRequestFallbackPolicy, HttpRetryPolicy, TimeoutPolicy);
        }
    }
}
