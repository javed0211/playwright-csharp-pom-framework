using Polly;
using Polly.Retry;

namespace PlaywrightPom.Core.Resilience;

public static class TransientRetry
{
    public static ResiliencePipeline Create(int extraAttempts, TimeSpan? delay = null)
    {
        return new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = Math.Max(0, extraAttempts),
                Delay = delay ?? TimeSpan.FromMilliseconds(200),
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = new PredicateBuilder().Handle<TransientRequestException>(),
            })
            .Build();
    }
}

public sealed class TransientRequestException : Exception
{
    public TransientRequestException(string message)
        : base(message)
    {
    }

    public TransientRequestException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
