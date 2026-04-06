namespace Whycespace.Domain.ConstitutionalSystem.Policy.Version;

using Whycespace.Domain.SharedKernel;

public sealed class PolicyVersionAggregate : AggregateRoot
{
    public Guid PolicyRuleId { get; private set; }
    public int VersionNumber { get; private set; }
    public EffectiveDateRange EffectiveDateRange { get; private set; } = default!;
    public VersionStatus Status { get; private set; } = default!;
    public string ChangeDescription { get; private set; } = string.Empty;
    public PolicyArtifactId? ArtifactId { get; private set; }
    public string? ArtifactHash { get; private set; }
    public string? ArtifactLocation { get; private set; }
    public bool IsLocked { get; private set; }
    public DateTimeOffset? LockedAt { get; private set; }
    public IReadOnlyList<Guid>? LockedBy { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private PolicyVersionAggregate() { }

    public static PolicyVersionAggregate Create(
        Guid versionId,
        Guid policyRuleId,
        int versionNumber,
        EffectiveDateRange effectiveDateRange,
        string changeDescription,
        DateTimeOffset timestamp)
    {
        if (versionNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(versionNumber), "Version number must be positive.");
        ArgumentNullException.ThrowIfNull(effectiveDateRange);
        ArgumentException.ThrowIfNullOrWhiteSpace(changeDescription);

        var version = new PolicyVersionAggregate
        {
            Id = versionId,
            PolicyRuleId = policyRuleId,
            VersionNumber = versionNumber,
            EffectiveDateRange = effectiveDateRange,
            Status = VersionStatus.Draft,
            ChangeDescription = changeDescription,
            CreatedAt = timestamp
        };

        version.RaiseDomainEvent(new PolicyVersionCreatedEvent(
            version.Id, policyRuleId, versionNumber));

        return version;
    }

    public void Activate()
    {
        if (Status != VersionStatus.Draft)
            throw new InvalidOperationException("Only draft versions can be activated.");

        Status = VersionStatus.Active;
        RaiseDomainEvent(new PolicyVersionActivatedEvent(Id, PolicyRuleId, VersionNumber));
    }

    public void Supersede()
    {
        if (Status != VersionStatus.Active)
            throw new InvalidOperationException("Only active versions can be superseded.");

        Status = VersionStatus.Superseded;
    }

    public void Archive()
    {
        if (Status == VersionStatus.Archived)
            throw new InvalidOperationException("Version is already archived.");

        Status = VersionStatus.Archived;
    }

    public void LinkArtifact(string artifactHash, string artifactLocation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(artifactHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(artifactLocation);

        ArtifactId = PolicyArtifactId.Generate(PolicyRuleId, VersionNumber);
        ArtifactHash = artifactHash;
        ArtifactLocation = artifactLocation;
    }

    public void Lock(IReadOnlyList<Guid> lockedBy, DateTimeOffset timestamp)
    {
        if (IsLocked) throw new InvalidOperationException("Version is already locked.");
        if (Status != VersionStatus.Active) throw new InvalidOperationException("Only active versions can be locked.");
        if (lockedBy.Count == 0) throw new ArgumentException("Lock requires at least one signer.");

        IsLocked = true;
        LockedAt = timestamp;
        LockedBy = lockedBy;
    }

    public bool HasArtifact => ArtifactId is not null;

    public bool IsEffectiveAt(DateTimeOffset pointInTime)
    {
        return Status == VersionStatus.Active && EffectiveDateRange.Contains(pointInTime);
    }
}
