using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Streaming.Distribution;

public sealed record DistributionChannelAddedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    DistributionPolicyId PolicyId, string Channel, Timestamp AddedAt) : DomainEvent;
