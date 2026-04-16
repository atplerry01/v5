using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Metadata;

public sealed record MetadataTaggedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    MetadataId MetadataId, string Tag, Timestamp TaggedAt) : DomainEvent;
