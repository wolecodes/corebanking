using Polly;

namespace CoreBanking.Infrastructure.External.Resilience;

public class AdvancedPollyPolicies
{
  public AdvancedPollyPolicies() { }

  // HTTP-SPECIFIC POLICIES (for HttpClient calls)
  public IAsyncPolicy<HttpResponseMessage> CreateHttpJitterRetryPolicy()
  {
    var jitterer = new Random();
    return Policy<HttpResponseMessage>
        .Handle<HttpRequestException>()
        .Or<TimeoutException>()
        .OrResult(r => (int)r.StatusCode >= 500)
        .WaitAndRetryAsync(5, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) +
            TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)));
  }

  public IAsyncPolicy<HttpResponseMessage> CreateHttpResiliencePipeline()
  {
    return Policy.WrapAsync(
        CreateHttpJitterRetryPolicy(),
        CreateHttpCircuitBreakerPolicy(),
        Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(15))
    );
  }

  // GENERIC POLICIES (for any operation)
  public IAsyncPolicy<T> CreateGenericResiliencePipeline<T>()
  {
    var jitterer = new Random();

    var retryPolicy = Policy<T>
        .Handle<Exception>()
        .WaitAndRetryAsync(5, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) +
            TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)));

    var circuitBreaker = Policy<T>
        .Handle<Exception>()
        .CircuitBreakerAsync(3, TimeSpan.FromSeconds(30));

    var timeoutPolicy = Policy.TimeoutAsync<T>(TimeSpan.FromSeconds(15));

    return Policy.WrapAsync(retryPolicy, circuitBreaker, timeoutPolicy);
  }

  private IAsyncPolicy<HttpResponseMessage> CreateHttpCircuitBreakerPolicy()
  {
    return Policy<HttpResponseMessage>
        .Handle<HttpRequestException>()
        .OrResult(r => (int)r.StatusCode >= 500)
        .CircuitBreakerAsync(3, TimeSpan.FromSeconds(30));
  }
}