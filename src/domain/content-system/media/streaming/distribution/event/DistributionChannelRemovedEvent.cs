using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Streaming.Distribution;

public sealed record DistributionChannelRemovedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    DistributionPolicyId PolicyId, string Channel, Timestamp RemovedAt) : DomainEvent;
