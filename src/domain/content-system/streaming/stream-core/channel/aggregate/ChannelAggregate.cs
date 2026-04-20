using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Channel;

public sealed class ChannelAggregate : AggregateRoot
{
    public ChannelId ChannelId { get; private set; }
    public StreamRef StreamRef { get; private set; }
    public ChannelName Name { get; private set; }
    public ChannelMode Mode { get; private set; }
    public ChannelStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }

    private ChannelAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static ChannelAggregate Create(
        ChannelId channelId,
        StreamRef streamRef,
        ChannelName name,
        ChannelMode mode,
        Timestamp createdAt)
    {
        var aggregate = new ChannelAggregate();

        aggregate.RaiseDomainEvent(new ChannelCreatedEvent(
            channelId,
            streamRef,
            name,
            mode,
            createdAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Rename(ChannelName newName, Timestamp renamedAt)
    {
        if (Status == ChannelStatus.Archived)
            throw ChannelErrors.ChannelArchived();

        if (Name == newName)
            return;

        RaiseDomainEvent(new ChannelRenamedEvent(ChannelId, Name, newName, renamedAt));
    }

    public void Enable(Timestamp enabledAt)
    {
        if (Status == ChannelStatus.Archived)
            throw ChannelErrors.ChannelArchived();

        if (Status == ChannelStatus.Enabled)
            throw ChannelErrors.ChannelAlreadyEnabled();

        RaiseDomainEvent(new ChannelEnabledEvent(ChannelId, enabledAt));
    }

    public void Disable(string reason, Timestamp disabledAt)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw ChannelErrors.InvalidDisableReason();

        if (Status == ChannelStatus.Archived)
            throw ChannelErrors.ChannelArchived();

        if (Status == ChannelStatus.Disabled)
            throw ChannelErrors.ChannelAlreadyDisabled();

        RaiseDomainEvent(new ChannelDisabledEvent(ChannelId, reason.Trim(), disabledAt));
    }

    public void Archive(Timestamp archivedAt)
    {
        if (Status == ChannelStatus.Archived)
            throw ChannelErrors.ChannelAlreadyArchived();

        RaiseDomainEvent(new ChannelArchivedEvent(ChannelId, archivedAt));
    }

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ChannelCreatedEvent e:
                ChannelId = e.ChannelId;
                StreamRef = e.StreamRef;
                Name = e.Name;
                Mode = e.Mode;
                Status = ChannelStatus.Created;
                CreatedAt = e.CreatedAt;
                LastModifiedAt = e.CreatedAt;
                break;

            case ChannelRenamedEvent e:
                Name = e.NewName;
                LastModifiedAt = e.RenamedAt;
                break;

            case ChannelEnabledEvent e:
                Status = ChannelStatus.Enabled;
                LastModifiedAt = e.EnabledAt;
                break;

            case ChannelDisabledEvent e:
                Status = ChannelStatus.Disabled;
                LastModifiedAt = e.DisabledAt;
                break;

            case ChannelArchivedEvent e:
                Status = ChannelStatus.Archived;
                LastModifiedAt = e.ArchivedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (StreamRef.Value == Guid.Empty)
            throw ChannelErrors.OrphanedChannel();
    }
}
