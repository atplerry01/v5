using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Version;

public sealed record AssetVersionDraftedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    AssetVersionId VersionId, string AssetRef, int Major, int Minor, int Patch, Timestamp DraftedAt) : DomainEvent;
