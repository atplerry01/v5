using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Streaming.StreamSession;

public sealed class StreamSessionAggregate : AggregateRoot
{
    private static readonly StreamSessionSpecification Spec = new();
    private readonly Dictionary<string, Viewer> _viewers = new(StringComparer.Ordinal);

    public StreamSessionId SessionId { get; private set; }
    public string AssetRef { get; private set; } = string.Empty;
    public StreamEndpoint Endpoint { get; private set; } = default!;
    public StreamSessionStatus Status { get; private set; }
    public Timestamp OpenedAt { get; private set; }
    public IReadOnlyCollection<Viewer> Viewers => _viewers.Values;

    private StreamSessionAggregate() { }

    public static StreamSessionAggregate Open(
        EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId,
        StreamSessionId id, string assetRef, StreamEndpoint endpoint, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(assetRef)) throw StreamSessionErrors.InvalidAssetRef();
        var agg = new StreamSessionAggregate();
        agg.RaiseDomainEvent(new StreamOpenedEvent(eventId, aggregateId, correlationId, causationId, id, assetRef, endpoint.Uri, at));
        return agg;
    }

    public void JoinViewer(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, string viewerRef, Timestamp at)
    {
        Spec.EnsureOpen(Status);
        if (string.IsNullOrWhiteSpace(viewerRef)) throw StreamSessionErrors.InvalidViewer();
        if (_viewers.TryGetValue(viewerRef, out var v) && v.IsActive) return;
        RaiseDomainEvent(new StreamViewerJoinedEvent(eventId, aggregateId, correlationId, causationId, SessionId, viewerRef, at));
    }

    public void LeaveViewer(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, string viewerRef, Timestamp at)
    {
        if (!_viewers.TryGetValue(viewerRef, out var v) || !v.IsActive)
            throw StreamSessionErrors.ViewerNotInSession();
        RaiseDomainEvent(new StreamViewerLeftEvent(eventId, aggregateId, correlationId, causationId, SessionId, viewerRef, at));
    }

    public void Close(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        Spec.EnsureNotTerminal(Status);
        RaiseDomainEvent(new StreamClosedEvent(eventId, aggregateId, correlationId, causationId, SessionId, at));
    }

    public void Terminate(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, string reason, Timestamp at)
    {
        Spec.EnsureNotTerminal(Status);
        RaiseDomainEvent(new StreamTerminatedEvent(eventId, aggregateId, correlationId, causationId, SessionId, reason ?? string.Empty, at));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case StreamOpenedEvent e:
                SessionId = e.SessionId;
                AssetRef = e.AssetRef;
                Endpoint = StreamEndpoint.Create(e.EndpointUri);
                Status = StreamSessionStatus.Open;
                OpenedAt = e.OpenedAt;
                break;
            case StreamViewerJoinedEvent e:
                _viewers[e.ViewerRef] = Viewer.Join(e.ViewerRef, e.JoinedAt);
                break;
            case StreamViewerLeftEvent e:
                if (_viewers.TryGetValue(e.ViewerRef, out var v)) v.Leave(e.LeftAt);
                break;
            case StreamClosedEvent: Status = StreamSessionStatus.Closed; break;
            case StreamTerminatedEvent: Status = StreamSessionStatus.Terminated; break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (DomainEvents.Count > 0 && string.IsNullOrEmpty(AssetRef))
            throw StreamSessionErrors.AssetMissing();
    }
}
