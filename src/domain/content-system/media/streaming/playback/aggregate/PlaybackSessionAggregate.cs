using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Streaming.Playback;

public sealed class PlaybackSessionAggregate : AggregateRoot
{
    private static readonly PlaybackSpecification Spec = new();

    public PlaybackSessionId SessionId { get; private set; }
    public string AssetRef { get; private set; } = string.Empty;
    public string ViewerRef { get; private set; } = string.Empty;
    public PlaybackStatus Status { get; private set; }
    public PlaybackPosition Position { get; private set; } = PlaybackPosition.Zero;
    public Timestamp StartedAt { get; private set; }
    public Timestamp? TerminatedAt { get; private set; }

    private PlaybackSessionAggregate() { }

    public static PlaybackSessionAggregate Start(
        EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId,
        PlaybackSessionId id, string assetRef, string viewerRef, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(assetRef)) throw PlaybackErrors.InvalidAssetRef();
        if (string.IsNullOrWhiteSpace(viewerRef)) throw PlaybackErrors.InvalidViewerRef();
        var agg = new PlaybackSessionAggregate();
        agg.RaiseDomainEvent(new PlaybackStartedEvent(eventId, aggregateId, correlationId, causationId, id, assetRef, viewerRef, at));
        return agg;
    }

    public void Pause(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, PlaybackPosition position, Timestamp at)
    {
        Spec.EnsureTransition(Status, PlaybackStatus.Paused);
        RaiseDomainEvent(new PlaybackPausedEvent(eventId, aggregateId, correlationId, causationId, SessionId, position.Milliseconds, at));
    }

    public void Resume(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, PlaybackPosition position, Timestamp at)
    {
        if (Status != PlaybackStatus.Paused) throw PlaybackErrors.NotPaused();
        RaiseDomainEvent(new PlaybackResumedEvent(eventId, aggregateId, correlationId, causationId, SessionId, position.Milliseconds, at));
    }

    public void Complete(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        Spec.EnsureTransition(Status, PlaybackStatus.Completed);
        RaiseDomainEvent(new PlaybackCompletedEvent(eventId, aggregateId, correlationId, causationId, SessionId, at));
    }

    public void Stop(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        Spec.EnsureTransition(Status, PlaybackStatus.Stopped);
        RaiseDomainEvent(new PlaybackStoppedEvent(eventId, aggregateId, correlationId, causationId, SessionId, at));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case PlaybackStartedEvent e:
                SessionId = e.SessionId;
                AssetRef = e.AssetRef;
                ViewerRef = e.ViewerRef;
                Status = PlaybackStatus.Started;
                StartedAt = e.StartedAt;
                break;
            case PlaybackPausedEvent e:
                Status = PlaybackStatus.Paused;
                Position = PlaybackPosition.Create(e.PositionMs);
                break;
            case PlaybackResumedEvent e:
                Status = PlaybackStatus.Resumed;
                Position = PlaybackPosition.Create(e.PositionMs);
                break;
            case PlaybackCompletedEvent e:
                Status = PlaybackStatus.Completed;
                TerminatedAt = e.CompletedAt;
                break;
            case PlaybackStoppedEvent e:
                Status = PlaybackStatus.Stopped;
                TerminatedAt = e.StoppedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (DomainEvents.Count > 0 && string.IsNullOrEmpty(AssetRef))
            throw PlaybackErrors.AssetMissing();
    }
}
