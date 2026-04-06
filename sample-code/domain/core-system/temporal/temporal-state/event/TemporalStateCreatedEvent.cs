using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.Temporal.TemporalState;

public sealed record TemporalStateCreatedEvent(Guid StateId, Guid EntityId, DateTimeOffset EffectiveFrom) : DomainEvent;
public sealed record TemporalStateSupersededEvent(Guid StateId, DateTimeOffset NewEffectiveFrom) : DomainEvent;
