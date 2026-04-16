using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Asset;

public sealed record MediaAssetMetadataUpdatedEvent(
    EventId EventId,
    AggregateId AggregateId,
    CorrelationId CorrelationId,
    CausationId CausationId,
    MediaAssetId MediaAssetId,
    string Title,
    string Description,
    IReadOnlyList<string> Tags,
    Timestamp UpdatedAt) : DomainEvent;
