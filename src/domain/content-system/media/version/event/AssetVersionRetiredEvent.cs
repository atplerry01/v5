using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Version;

public sealed record AssetVersionRetiredEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    AssetVersionId VersionId, Timestamp RetiredAt) : DomainEvent;
