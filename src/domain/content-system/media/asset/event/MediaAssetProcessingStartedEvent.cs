using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Asset;

public sealed record MediaAssetProcessingStartedEvent(
    EventId EventId,
    AggregateId AggregateId,
    CorrelationId CorrelationId,
    CausationId CausationId,
    MediaAssetId MediaAssetId,
    Timestamp StartedAt) : DomainEvent;
