using Whycespace.Domain.SharedKernel.Primitive.Time;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Domain.SharedKernel.Primitives.Kernel;

public abstract class AggregateRoot : IEventSource
{
    public Guid Id { get; protected set; }
    private readonly List<DomainEvent> _domainEvents = [];
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Injected clock for deterministic timestamp generation.
    /// Set by the runtime via SetClock() before command execution.
    /// Falls back to SystemClock if not explicitly set (test/migration scenarios).
    /// </summary>
    private IClock? _clock;

    public void SetClock(IClock clock) => _clock = clock;

    protected void RaiseDomainEvent(DomainEvent domainEvent)
    {
        // Sequence key: aggregate event count ensures uniqueness within same aggregate + event type
        var sequenceKey = _domainEvents.Count.ToString();
        var eventType = domainEvent.GetType().Name;

        // Use injected clock for deterministic timestamps; fall back to SystemClock
        var occurredAt = domainEvent.OccurredAt == DateTimeOffset.MinValue
            ? (_clock?.UtcNowOffset ?? SystemClock.Instance.UtcNowOffset)
            : domainEvent.OccurredAt;

        var enriched = domainEvent with
        {
            AggregateId = Id,
            AggregateType = GetType().Name,
            EventId = EventId.Deterministic(Id, eventType, domainEvent.Version, sequenceKey),
            OccurredAt = occurredAt
        };
        _domainEvents.Add(enriched);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    // IEventSource — type-erased bridge for EngineContext
    IReadOnlyList<object> IEventSource.GetPendingEvents() => _domainEvents.Cast<object>().ToList().AsReadOnly();
    void IEventSource.ClearPendingEvents() => ClearDomainEvents();

    /// <summary>
    /// Asserts that a domain invariant holds. Throws <see cref="DomainInvariantViolationException"/>
    /// if the condition is false.
    /// </summary>
    protected void EnsureInvariant(bool condition, string invariant, string message)
    {
        if (!condition)
            throw new DomainInvariantViolationException(
                GetType().Name, invariant, message, Id == Guid.Empty ? null : Id);
    }

    /// <summary>
    /// Guards against performing an action when the aggregate is in a terminal state.
    /// </summary>
    protected void EnsureNotTerminal<TStatus>(TStatus status, Func<TStatus, bool> isTerminal, string action)
    {
        if (isTerminal(status))
            throw DomainInvariantViolationException.TerminalState(
                GetType().Name, status?.ToString() ?? "unknown", action, Id == Guid.Empty ? null : Id);
    }

    /// <summary>
    /// Guards against an invalid state transition.
    /// </summary>
    protected void EnsureValidTransition<TStatus>(TStatus from, TStatus to, Func<TStatus, TStatus, bool> isValid)
        where TStatus : notnull
    {
        if (!isValid(from, to))
            throw DomainInvariantViolationException.InvalidStateTransition(
                GetType().Name, from.ToString()!, to.ToString()!, Id == Guid.Empty ? null : Id);
    }
}
