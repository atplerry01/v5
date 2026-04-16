using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Engagement.Search;

public sealed record SearchDocumentIndexedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    SearchIndexId IndexId, string DocumentRef, string NormalisedText, Timestamp IndexedAt) : DomainEvent;
