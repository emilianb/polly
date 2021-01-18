using Polly;
using Polly.Retry;
using System;
using System.Net.Http;

namespace PollySamples.Controllers.SharePoliciesByDISample
{
    public class PolicyHolder
    {
        public AsyncRetryPolicy<HttpResponseMessage> HttpRetryPolicy { get; private set; }

        public AsyncRetryPolicy HttpClientTimeoutException { get; private set; }

        public PolicyHolder()
        {
            HttpRetryPolicy = Policy
                .HandleResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    3,
                    retryCount => TimeSpan.FromSeconds(retryCount),
                    (response, timespan) =>
                    {
                        var result = response.Result;

                        // log the result.
                    }
                );

            HttpClientTimeoutException = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(
                    1,
                    retryCount => TimeSpan.FromSeconds(retryCount),
                    onRetry: (exception, timespan) =>
                    {
                        var message = exception.Message;

                        // log the message.
                    }
                );
        }
    }
}
