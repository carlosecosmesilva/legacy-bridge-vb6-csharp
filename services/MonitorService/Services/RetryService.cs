using MonitorService.Models;

namespace MonitorService.Services;

public class RetryService
{
    private readonly ILogger<RetryService> _logger;
    private readonly Random _random = new();

    public RetryService(ILogger<RetryService> logger) => _logger = logger;

    public async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        RetryPolicy policy,
        CancellationToken cancellationToken = default)
    {
        var attempt = 0;
        Exception? lastException = null;

        while (attempt < policy.MaxAttempts)
        {
            try
            {
                if (policy.Timeout.HasValue)
                {
                    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    timeoutCts.CancelAfter(policy.Timeout.Value);
                    
                    return await operation();
                }
                else
                {
                    return await operation();
                }
            }
            catch (Exception ex) when (attempt < policy.MaxAttempts - 1)
            {
                lastException = ex;
                attempt++;
                
                var delay = CalculateDelay(attempt, policy);
                _logger.LogWarning(ex, "Operation failed, retrying in {Delay}ms (attempt {Attempt}/{MaxAttempts})", 
                    delay.TotalMilliseconds, attempt, policy.MaxAttempts);
                
                await Task.Delay(delay, cancellationToken);
            }
        }

        _logger.LogError(lastException, "Operation failed after {MaxAttempts} attempts", policy.MaxAttempts);
        throw lastException ?? new InvalidOperationException("Operation failed");
    }

    public async Task ExecuteWithRetryAsync(
        Func<Task> operation,
        RetryPolicy policy,
        CancellationToken cancellationToken = default)
    {
        await ExecuteWithRetryAsync(async () =>
        {
            await operation();
            return true;
        }, policy, cancellationToken);
    }

    private TimeSpan CalculateDelay(int attempt, RetryPolicy policy)
    {
        var delay = TimeSpan.FromMilliseconds(
            policy.InitialDelay.TotalMilliseconds * Math.Pow(policy.BackoffMultiplier, attempt - 1));
        
        delay = TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds, policy.MaxDelay.TotalMilliseconds));
        
        if (policy.Jitter)
        {
            var jitterRange = delay.TotalMilliseconds * 0.1; // 10% jitter
            var jitter = (_random.NextDouble() - 0.5) * 2 * jitterRange;
            delay = TimeSpan.FromMilliseconds(Math.Max(0, delay.TotalMilliseconds + jitter));
        }
        
        return delay;
    }
}
