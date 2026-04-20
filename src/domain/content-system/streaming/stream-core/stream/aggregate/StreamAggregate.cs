using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Stream;

public sealed class StreamAggregate : AggregateRoot
{
    public StreamId StreamId { get; private set; }
    public StreamMode Mode { get; private set; }
    public StreamType Type { get; private set; }
    public StreamStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public Timestamp? StartedAt { get; private set; }
    public Timestamp? EndedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private StreamAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static StreamAggregate Create(
        StreamId streamId,
        StreamMode mode,
        StreamType type,
        Timestamp createdAt)
    {
        var aggregate = new StreamAggregate();

        aggregate.RaiseDomainEvent(new StreamCreatedEvent(streamId, mode, type, createdAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Activate(Timestamp activatedAt)
    {
        if (Status == StreamStatus.Active)
            throw StreamErrors.StreamAlreadyActive();

        if (Status == StreamStatus.Ended)
            throw StreamErrors.StreamAlreadyEnded();

        if (Status == StreamStatus.Archived)
            throw StreamErrors.InvalidTransition(Status, StreamStatus.Active);

        RaiseDomainEvent(new StreamActivatedEvent(StreamId, activatedAt));
    }

    public void Pause(Timestamp pausedAt)
    {
        if (Status != StreamStatus.Active)
            throw StreamErrors.StreamNotActive();

        RaiseDomainEvent(new StreamPausedEvent(StreamId, pausedAt));
    }

    public void Resume(Timestamp resumedAt)
    {
        if (Status != StreamStatus.Paused)
            throw StreamErrors.StreamNotPaused();

        RaiseDomainEvent(new StreamResumedEvent(StreamId, resumedAt));
    }

    public void End(Timestamp endedAt)
    {
        if (Status == StreamStatus.Ended)
            throw StreamErrors.StreamAlreadyEnded();

        if (Status == StreamStatus.Archived)
            throw StreamErrors.InvalidTransition(Status, StreamStatus.Ended);

        if (Status != StreamStatus.Active && Status != StreamStatus.Paused)
            throw StreamErrors.InvalidTransition(Status, StreamStatus.Ended);

        RaiseDomainEvent(new StreamEndedEvent(StreamId, endedAt));
    }

    public void Archive(Timestamp archivedAt)
    {
        if (Status == StreamStatus.Archived)
            throw StreamErrors.StreamAlreadyArchived();

        if (Status != StreamStatus.Ended)
            throw StreamErrors.StreamNotEnded();

        RaiseDomainEvent(new StreamArchivedEvent(StreamId, archivedAt));
    }

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case StreamCreatedEvent e:
                StreamId = e.StreamId;
                Mode = e.Mode;
                Type = e.Type;
                Status = StreamStatus.Created;
                CreatedAt = e.CreatedAt;
                LastModifiedAt = e.CreatedAt;
                break;

            case StreamActivatedEvent e:
                Status = StreamStatus.Active;
                StartedAt ??= e.ActivatedAt;
                LastModifiedAt = e.ActivatedAt;
                break;

            case StreamPausedEvent e:
                Status = StreamStatus.Paused;
                LastModifiedAt = e.PausedAt;
                break;

            case StreamResumedEvent e:
                Status = StreamStatus.Active;
                LastModifiedAt = e.ResumedAt;
                break;

            case StreamEndedEvent e:
                Status = StreamStatus.Ended;
                EndedAt = e.EndedAt;
                LastModifiedAt = e.EndedAt;
                break;

            case StreamArchivedEvent e:
                Status = StreamStatus.Archived;
                LastModifiedAt = e.ArchivedAt;
                break;
        }
    }
}
