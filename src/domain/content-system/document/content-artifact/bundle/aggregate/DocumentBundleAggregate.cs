using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.Bundle;

public sealed class DocumentBundleAggregate : AggregateRoot
{
    private readonly HashSet<BundleMemberRef> _members = new();

    public DocumentBundleId BundleId { get; private set; }
    public BundleName Name { get; private set; }
    public BundleStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public Timestamp LastModifiedAt { get; private set; }
    public Timestamp? FinalizedAt { get; private set; }

    public IReadOnlyCollection<BundleMemberRef> Members => _members;

    private DocumentBundleAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static DocumentBundleAggregate Create(
        DocumentBundleId bundleId,
        BundleName name,
        Timestamp createdAt)
    {
        var aggregate = new DocumentBundleAggregate();

        aggregate.RaiseDomainEvent(new DocumentBundleCreatedEvent(bundleId, name, createdAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Rename(BundleName newName, Timestamp renamedAt)
    {
        EnsureMutable();

        if (Name == newName)
            return;

        RaiseDomainEvent(new DocumentBundleRenamedEvent(BundleId, Name, newName, renamedAt));
    }

    public void AddMember(BundleMemberRef member, Timestamp addedAt)
    {
        EnsureMutable();

        if (_members.Contains(member))
            throw DocumentBundleErrors.DuplicateMember();

        RaiseDomainEvent(new DocumentBundleMemberAddedEvent(BundleId, member, addedAt));
    }

    public void RemoveMember(BundleMemberRef member, Timestamp removedAt)
    {
        EnsureMutable();

        if (!_members.Contains(member))
            throw DocumentBundleErrors.UnknownMember();

        RaiseDomainEvent(new DocumentBundleMemberRemovedEvent(BundleId, member, removedAt));
    }

    public void Finalize(Timestamp finalizedAt)
    {
        if (Status == BundleStatus.Finalized)
            throw DocumentBundleErrors.AlreadyFinalized();

        if (Status == BundleStatus.Archived)
            throw DocumentBundleErrors.BundleArchived();

        if (_members.Count == 0)
            throw DocumentBundleErrors.EmptyBundle();

        RaiseDomainEvent(new DocumentBundleFinalizedEvent(BundleId, finalizedAt));
    }

    public void Archive(Timestamp archivedAt)
    {
        if (Status == BundleStatus.Archived)
            throw DocumentBundleErrors.AlreadyArchived();

        RaiseDomainEvent(new DocumentBundleArchivedEvent(BundleId, archivedAt));
    }

    private void EnsureMutable()
    {
        if (Status == BundleStatus.Finalized)
            throw DocumentBundleErrors.BundleFinalized();

        if (Status == BundleStatus.Archived)
            throw DocumentBundleErrors.BundleArchived();
    }

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case DocumentBundleCreatedEvent e:
                BundleId = e.BundleId;
                Name = e.Name;
                Status = BundleStatus.Open;
                CreatedAt = e.CreatedAt;
                LastModifiedAt = e.CreatedAt;
                break;

            case DocumentBundleRenamedEvent e:
                Name = e.NewName;
                LastModifiedAt = e.RenamedAt;
                break;

            case DocumentBundleMemberAddedEvent e:
                _members.Add(e.Member);
                LastModifiedAt = e.AddedAt;
                break;

            case DocumentBundleMemberRemovedEvent e:
                _members.Remove(e.Member);
                LastModifiedAt = e.RemovedAt;
                break;

            case DocumentBundleFinalizedEvent e:
                Status = BundleStatus.Finalized;
                FinalizedAt = e.FinalizedAt;
                LastModifiedAt = e.FinalizedAt;
                break;

            case DocumentBundleArchivedEvent e:
                Status = BundleStatus.Archived;
                LastModifiedAt = e.ArchivedAt;
                break;
        }
    }
}
