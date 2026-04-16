using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Version;

public sealed record AssetVersionSupersededEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    AssetVersionId VersionId, AssetVersionId SucceededBy, Timestamp SupersededAt) : DomainEvent;
