using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Availability;

public sealed class PlaybackAggregate : AggregateRoot
{
    public PlaybackId PlaybackId { get; private set; }
    public PlaybackSourceRef SourceRef { get; private set; }
    public PlaybackMode Mode { get; private set; }
    public PlaybackWindow Window { get; private set; }
    public PlaybackStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private PlaybackAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static PlaybackAggregate Create(
        PlaybackId playbackId,
        PlaybackSourceRef sourceRef,
        PlaybackMode mode,
        PlaybackWindow window,
        Timestamp createdAt)
    {
        var aggregate = new PlaybackAggregate();

        aggregate.RaiseDomainEvent(new PlaybackCreatedEvent(
            playbackId,
            sourceRef,
            mode,
            window,
            createdAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Enable(Timestamp enabledAt)
    {
        if (Status == PlaybackStatus.Archived)
            throw PlaybackErrors.PlaybackArchived();

        if (Status == PlaybackStatus.Enabled)
            throw PlaybackErrors.PlaybackAlreadyEnabled();

        RaiseDomainEvent(new PlaybackEnabledEvent(PlaybackId, enabledAt));
    }

    public void Disable(string reason, Timestamp disabledAt)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw PlaybackErrors.InvalidDisableReason();

        if (Status == PlaybackStatus.Archived)
            throw PlaybackErrors.PlaybackArchived();

        if (Status == PlaybackStatus.Disabled)
            throw PlaybackErrors.PlaybackAlreadyDisabled();

        RaiseDomainEvent(new PlaybackDisabledEvent(PlaybackId, reason.Trim(), disabledAt));
    }

    public void UpdateWindow(PlaybackWindow newWindow, Timestamp updatedAt)
    {
        if (Status == PlaybackStatus.Archived)
            throw PlaybackErrors.PlaybackArchived();

        if (Window == newWindow)
            return;

        RaiseDomainEvent(new PlaybackWindowUpdatedEvent(PlaybackId, Window, newWindow, updatedAt));
    }

    public void Archive(Timestamp archivedAt)
    {
        if (Status == PlaybackStatus.Archived)
            throw PlaybackErrors.PlaybackAlreadyArchived();

        RaiseDomainEvent(new PlaybackArchivedEvent(PlaybackId, archivedAt));
    }

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case PlaybackCreatedEvent e:
                PlaybackId = e.PlaybackId;
                SourceRef = e.SourceRef;
                Mode = e.Mode;
                Window = e.Window;
                Status = PlaybackStatus.Created;
                CreatedAt = e.CreatedAt;
                LastModifiedAt = e.CreatedAt;
                break;

            case PlaybackEnabledEvent e:
                Status = PlaybackStatus.Enabled;
                LastModifiedAt = e.EnabledAt;
                break;

            case PlaybackDisabledEvent e:
                Status = PlaybackStatus.Disabled;
                LastModifiedAt = e.DisabledAt;
                break;

            case PlaybackWindowUpdatedEvent e:
                Window = e.NewWindow;
                LastModifiedAt = e.UpdatedAt;
                break;

            case PlaybackArchivedEvent e:
                Status = PlaybackStatus.Archived;
                LastModifiedAt = e.ArchivedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (SourceRef.Value == Guid.Empty)
            throw PlaybackErrors.OrphanedPlayback();
    }
}
