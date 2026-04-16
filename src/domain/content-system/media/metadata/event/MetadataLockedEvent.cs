using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Metadata;

public sealed record MetadataLockedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    MetadataId MetadataId, Timestamp LockedAt) : DomainEvent;
