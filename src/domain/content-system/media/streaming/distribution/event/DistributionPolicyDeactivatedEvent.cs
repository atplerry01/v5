using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Streaming.Distribution;

public sealed record DistributionPolicyDeactivatedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    DistributionPolicyId PolicyId, Timestamp DeactivatedAt) : DomainEvent;
