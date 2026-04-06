namespace Whycespace.Domain.SharedKernel.Primitives.Kernel;

/// <summary>
/// Base contract for all domain events. Immutable, serializable, and replay-safe.
/// All events flowing through Kafka or event stores MUST inherit from this record.
///
/// EventId is DETERMINISTIC — derived from SHA256(AggregateId + EventType + Version + SequenceKey).
/// The EventId is initially set to Empty and finalized by AggregateRoot.RaiseDomainEvent()
/// once the AggregateId is known. This guarantees same command replay produces identical EventIds.
/// </summary>
public abstract record DomainEvent
{
    public EventId EventId { get; init; } = EventId.Empty;
    public int Version { get; init; } = 1;
    public string SchemaId { get; init; }
    /// <summary>
    /// Timestamp of when the event occurred. MUST be set explicitly by the caller
    /// via the aggregate's RaiseDomainEvent enrichment or command context.
    /// Defaults to MinValue to detect unset timestamps during validation.
    /// </summary>
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.MinValue;
    public CorrelationId CorrelationId { get; init; } = CorrelationId.Empty;
    public CausationId CausationId { get; init; } = CausationId.Empty;
    public TraceId TraceId { get; init; } = TraceId.Empty;
    public AggregateId AggregateId { get; init; } = AggregateId.Empty;
    public string AggregateType { get; init; } = string.Empty;

    protected DomainEvent()
    {
        SchemaId = GetType().FullName ?? GetType().Name;
    }
}
