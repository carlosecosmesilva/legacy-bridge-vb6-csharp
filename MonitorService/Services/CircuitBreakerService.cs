using MonitorService.Models;

namespace MonitorService.Services;

public class CircuitBreakerService(ILogger<CircuitBreakerService> logger)
{
    private readonly ILogger<CircuitBreakerService> _logger = logger;
    private readonly Dictionary<string, CircuitBreakerState> _circuits = new();
    private readonly object _lock = new();

    public async Task<T> ExecuteWithCircuitBreakerAsync<T>(
        string circuitName,
        Func<Task<T>> operation,
        CircuitBreakerPolicy policy,
        CancellationToken cancellationToken = default)
    {
        var circuit = GetOrCreateCircuit(circuitName, policy);
        
        if (circuit.State == CircuitState.Open)
        {
            if (DateTime.UtcNow - circuit.LastFailureTime < policy.DurationOfBreak)
            {
                _logger.LogWarning("Circuit breaker is open for {CircuitName}", circuitName);
                throw new InvalidOperationException($"Circuit breaker is open for {circuitName}");
            }
            else
            {
                circuit.State = CircuitState.HalfOpen;
                _logger.LogInformation("Circuit breaker moved to half-open for {CircuitName}", circuitName);
            }
        }

        try
        {
            var result = await operation();
            OnSuccess(circuit);
            return result;
        }
        catch (Exception ex)
        {
            OnFailure(circuit, policy);
            _logger.LogError(ex, "Operation failed in circuit {CircuitName}", circuitName);
            throw;
        }
    }

    public async Task ExecuteWithCircuitBreakerAsync(
        string circuitName,
        Func<Task> operation,
        CircuitBreakerPolicy policy,
        CancellationToken cancellationToken = default)
    {
        await ExecuteWithCircuitBreakerAsync(circuitName, async () =>
        {
            await operation();
            return true;
        }, policy, cancellationToken);
    }

    public CircuitState GetCircuitState(string circuitName)
    {
        lock (_lock)
        {
            return _circuits.TryGetValue(circuitName, out var circuit) ? circuit.State : CircuitState.Closed;
        }
    }

    public void ResetCircuit(string circuitName)
    {
        lock (_lock)
        {
            if (_circuits.TryGetValue(circuitName, out var circuit))
            {
                circuit.State = CircuitState.Closed;
                circuit.FailureCount = 0;
                circuit.LastFailureTime = DateTime.MinValue;
                _logger.LogInformation("Circuit breaker reset for {CircuitName}", circuitName);
            }
        }
    }

    private CircuitBreakerState GetOrCreateCircuit(string circuitName, CircuitBreakerPolicy policy)
    {
        lock (_lock)
        {
            if (!_circuits.TryGetValue(circuitName, out var circuit))
            {
                circuit = new CircuitBreakerState
                {
                    Policy = policy,
                    State = CircuitState.Closed
                };
                _circuits[circuitName] = circuit;
            }
            return circuit;
        }
    }

    private void OnSuccess(CircuitBreakerState circuit)
    {
        lock (_lock)
        {
            circuit.FailureCount = 0;
            if (circuit.State == CircuitState.HalfOpen)
            {
                circuit.State = CircuitState.Closed;
                _logger.LogInformation("Circuit breaker closed after successful operation");
            }
        }
    }

    private void OnFailure(CircuitBreakerState circuit, CircuitBreakerPolicy policy)
    {
        lock (_lock)
        {
            circuit.FailureCount++;
            circuit.LastFailureTime = DateTime.UtcNow;

            if (circuit.FailureCount >= policy.FailureThreshold)
            {
                circuit.State = CircuitState.Open;
                _logger.LogWarning("Circuit breaker opened after {FailureCount} failures", circuit.FailureCount);
            }
        }
    }

    private class CircuitBreakerState
    {
        public CircuitState State { get; set; }
        public int FailureCount { get; set; }
        public DateTime LastFailureTime { get; set; }
        public CircuitBreakerPolicy Policy { get; set; } = new();
    }
}
