using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Engagement.Search;

public sealed class SearchIndexAggregate : AggregateRoot
{
    private static readonly SearchSpecification Spec = new();
    private readonly Dictionary<string, SearchDocument> _documents = new(StringComparer.Ordinal);

    public SearchIndexId IndexId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public SearchIndexStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public IReadOnlyCollection<SearchDocument> Documents => _documents.Values;

    private SearchIndexAggregate() { }

    public static SearchIndexAggregate Create(
        EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId,
        SearchIndexId id, string name, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(name)) throw SearchErrors.InvalidIndexName();
        var agg = new SearchIndexAggregate();
        agg.RaiseDomainEvent(new SearchIndexCreatedEvent(eventId, aggregateId, correlationId, causationId, id, name.Trim(), at));
        return agg;
    }

    public void IndexDocument(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, SearchDocument document, Timestamp at)
    {
        Spec.EnsureOpen(Status);
        if (_documents.ContainsKey(document.DocumentRef))
            throw SearchErrors.DocumentAlreadyIndexed(document.DocumentRef);
        RaiseDomainEvent(new SearchDocumentIndexedEvent(eventId, aggregateId, correlationId, causationId, IndexId, document.DocumentRef, document.NormalisedText, at));
    }

    public void Purge(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, string documentRef, Timestamp at)
    {
        Spec.EnsureOpen(Status);
        if (!_documents.ContainsKey(documentRef))
            throw SearchErrors.DocumentNotIndexed(documentRef);
        RaiseDomainEvent(new SearchDocumentPurgedEvent(eventId, aggregateId, correlationId, causationId, IndexId, documentRef, at));
    }

    public void Compact(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        Spec.EnsureOpen(Status);
        RaiseDomainEvent(new SearchIndexCompactedEvent(eventId, aggregateId, correlationId, causationId, IndexId, at));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case SearchIndexCreatedEvent e:
                IndexId = e.IndexId;
                Name = e.Name;
                Status = SearchIndexStatus.Open;
                CreatedAt = e.CreatedAt;
                break;
            case SearchDocumentIndexedEvent e:
                _documents[e.DocumentRef] = SearchDocument.Create(e.DocumentRef, e.NormalisedText);
                break;
            case SearchDocumentPurgedEvent e:
                _documents.Remove(e.DocumentRef);
                break;
            case SearchIndexCompactedEvent: Status = SearchIndexStatus.Compacted; break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (DomainEvents.Count > 0 && string.IsNullOrEmpty(Name))
            throw SearchErrors.NameMissing();
    }
}
