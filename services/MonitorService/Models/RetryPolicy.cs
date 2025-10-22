namespace MonitorService.Models;

public class RetryPolicy
{
    public int MaxAttempts { get; set; } = 3;
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(1);
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromMinutes(1);
    public double BackoffMultiplier { get; set; } = 2.0;
    public bool Jitter { get; set; } = true;
    public TimeSpan? Timeout { get; set; }
}

public class CircuitBreakerPolicy
{
    public int FailureThreshold { get; set; } = 5;
    public TimeSpan DurationOfBreak { get; set; } = TimeSpan.FromMinutes(1);
    public TimeSpan SamplingDuration { get; set; } = TimeSpan.FromMinutes(2);
    public int MinimumThroughput { get; set; } = 2;
}

public enum CircuitState
{
    Closed,
    Open,
    HalfOpen
}
