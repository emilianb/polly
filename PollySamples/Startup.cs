using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Bulkhead;
using Polly.CircuitBreaker;
using Polly.Registry;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PollySamples
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(GetAsyncBulkhead());
            // the purpose of the holder is to differentiate between the two registered circuit breaker
            services.AddSingleton(new Controllers.AdvancedCircuitBreakerSample.AdvancedCircuitBreakerHolder { BreakerPolicy = GetAdvancedAsyncCircuitBreaker() });
            services.AddSingleton(GetAsyncCircuitBreaker());
            services.AddSingleton(GetClient());
            services.AddSingleton<Controllers.WrappingReuseSample.IPolicyHolder>(new Controllers.WrappingReuseSample.PolicyHolder());
            services.AddSingleton(GetRegistry());
            services.AddSingleton(new Controllers.SharePoliciesByDISample.PolicyHolder());
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private AsyncBulkheadPolicy<HttpResponseMessage> GetAsyncBulkhead()
        {
            return Policy.BulkheadAsync<HttpResponseMessage>(2, 4, onBulkheadRejectedAsync: OnBulkheadRejectedAsync);
        }

        private Task OnBulkheadRejectedAsync(Context context)
        {
            Debug.WriteLine($"PollyDemo OnBulkheadRejectedAsync Executed");

            return Task.CompletedTask;
        }

        private AsyncCircuitBreakerPolicy<HttpResponseMessage> GetAdvancedAsyncCircuitBreaker()
        {
            return Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(60), 7, TimeSpan.FromSeconds(15), OnBreak, OnReset, OnHalfOpen);
        }

        private AsyncCircuitBreakerPolicy<HttpResponseMessage> GetAsyncCircuitBreaker()
        {
            return Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .CircuitBreakerAsync(2, TimeSpan.FromSeconds(60), OnBreak, OnReset, OnHalfOpen);
        }

        private void OnHalfOpen()
        {
            Debug.WriteLine("Connection half open");
        }

        private void OnReset(Context context)
        {
            Debug.WriteLine("Connection reset");
        }

        private void OnBreak(DelegateResult<HttpResponseMessage> delegateResult, TimeSpan timeSpan, Context context)
        {
            Debug.WriteLine($"Connection break: {delegateResult.Result}, {delegateResult.Result}");
        }

        private PolicyRegistry GetRegistry()
        {
            var registry = new PolicyRegistry();

            IAsyncPolicy<HttpResponseMessage> simpleHttpWaitAndRetry = Policy
                .HandleResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, retryCount => TimeSpan.FromSeconds(retryCount));

            registry.Add("SimpleHttpWaitAndRetry", simpleHttpWaitAndRetry);

            IAsyncPolicy httpClientTimeoutException = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(1, retryCount => TimeSpan.FromSeconds(retryCount));

            registry.Add("HttpClientTimeoutException", httpClientTimeoutException);

            return registry;
        }

        private HttpClient GetClient()
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(@"https://localhost:5001/api/");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return httpClient;
        }
    }
}
