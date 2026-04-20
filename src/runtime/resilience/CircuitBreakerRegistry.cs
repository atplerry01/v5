using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Runtime.Resilience;

/// <summary>
/// R2.A.D.4 / R-BREAKER-REGISTRY-01 — canonical <see cref="ICircuitBreakerRegistry"/>
/// implementation. Immutable after construction: breakers are registered
/// at host composition startup and never added/removed during runtime.
///
/// Enumeration order is sorted by <see cref="ICircuitBreaker.Name"/> so
/// health-posture reason lists are stable across process restarts.
/// </summary>
public sealed class CircuitBreakerRegistry : ICircuitBreakerRegistry
{
    private readonly IReadOnlyDictionary<string, ICircuitBreaker> _byName;
    private readonly IReadOnlyCollection<ICircuitBreaker> _sortedAll;

    /// <summary>
    /// Build from an enumerable of breakers. Throws <see cref="ArgumentException"/>
    /// if two breakers share the same <see cref="ICircuitBreaker.Name"/> —
    /// name collisions are composition-time bugs, not runtime concerns.
    /// </summary>
    public CircuitBreakerRegistry(IEnumerable<ICircuitBreaker> breakers)
    {
        ArgumentNullException.ThrowIfNull(breakers);

        var byName = new Dictionary<string, ICircuitBreaker>(StringComparer.Ordinal);
        foreach (var breaker in breakers)
        {
            ArgumentNullException.ThrowIfNull(breaker);
            if (string.IsNullOrWhiteSpace(breaker.Name))
                throw new ArgumentException(
                    "Circuit breaker has a null or blank Name; registry requires non-empty names.",
                    nameof(breakers));
            if (!byName.TryAdd(breaker.Name, breaker))
                throw new ArgumentException(
                    $"Duplicate circuit breaker name '{breaker.Name}' — registry names must be unique.",
                    nameof(breakers));
        }

        _byName = byName;
        _sortedAll = byName.Values
            .OrderBy(b => b.Name, StringComparer.Ordinal)
            .ToList()
            .AsReadOnly();
    }

    public ICircuitBreaker Get(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (_byName.TryGetValue(name, out var breaker))
            return breaker;
        throw new KeyNotFoundException(
            $"No circuit breaker registered under name '{name}'. " +
            $"Registered: [{string.Join(", ", _byName.Keys)}].");
    }

    public ICircuitBreaker? TryGet(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        _byName.TryGetValue(name, out var breaker);
        return breaker;
    }

    public IReadOnlyCollection<ICircuitBreaker> GetAll() => _sortedAll;
}
