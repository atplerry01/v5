using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Metadata;

public sealed record MetadataFieldUpdatedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    MetadataId MetadataId, string Key, string Value, Timestamp UpdatedAt) : DomainEvent;
