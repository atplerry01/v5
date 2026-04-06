using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

/// <summary>
/// Links an incident to the observability signal (alert, health check, etc.)
/// that led to its creation. Runtime/workflow decides escalation —
/// the domain only carries the correlation for traceability.
/// </summary>
public readonly record struct IncidentCorrelationId(string Value)
{
    public static IncidentCorrelationId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed).ToString());
    public static readonly IncidentCorrelationId Empty = new(string.Empty);

    public bool IsEmpty => string.IsNullOrEmpty(Value);

    /// <summary>
    /// Creates a correlation from an AlertTriggeredEvent's AlertId.
    /// </summary>
    public static IncidentCorrelationId FromAlert(Guid alertId)
        => new($"alert:{alertId}");

    /// <summary>
    /// Creates a correlation from a HealthDegradedEvent's ComponentId.
    /// </summary>
    public static IncidentCorrelationId FromHealth(string componentId)
        => new($"health:{componentId}");

    public override string ToString() => Value;

    public static implicit operator string(IncidentCorrelationId id) => id.Value;
    public static implicit operator IncidentCorrelationId(string id) => new(id);
}
