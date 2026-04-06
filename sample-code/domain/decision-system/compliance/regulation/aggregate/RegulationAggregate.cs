namespace Whycespace.Domain.DecisionSystem.Compliance.Regulation;

using Whycespace.Domain.DecisionSystem.Compliance.Regulation;
using Whycespace.Domain.SharedKernel;

public sealed class RegulationAggregate : AggregateRoot
{
    public string Title { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public RegulationType Type { get; private set; } = default!;
    public RegulationVersion CurrentVersion { get; private set; } = default!;
    public Guid JurisdictionId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset EnactedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    private RegulationAggregate() { }

    public static RegulationAggregate Create(
        Guid regulationId,
        string title,
        string description,
        RegulationType type,
        RegulationVersion version,
        Guid jurisdictionId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException(RegulationErrors.InvalidTitle, "Regulation title is required.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException(RegulationErrors.InvalidDescription, "Regulation description is required.");

        var regulation = new RegulationAggregate();
        var @event = new RegulationEnactedEvent(
            regulationId,
            title,
            description,
            type.Value,
            version.Major,
            version.Minor,
            version.Patch,
            jurisdictionId);

        regulation.Apply(@event);
        regulation.RaiseDomainEvent(@event);
        return regulation;
    }

    public void Amend(string description, RegulationVersion newVersion)
    {
        if (!IsActive)
            throw new DomainException(RegulationErrors.AlreadyRevoked, "Cannot amend a revoked regulation.");

        if (newVersion.CompareTo(CurrentVersion) <= 0)
            throw new DomainException(RegulationErrors.InvalidVersion, "New version must be greater than current version.");

        var @event = new RegulationAmendedEvent(
            Id,
            description,
            newVersion.Major,
            newVersion.Minor,
            newVersion.Patch);

        Apply(@event);
        RaiseDomainEvent(@event);
    }

    public void Revoke(string reason)
    {
        if (!IsActive)
            throw new DomainException(RegulationErrors.AlreadyRevoked, "Regulation is already revoked.");

        var @event = new RegulationRevokedEvent(Id, reason);
        Apply(@event);
        RaiseDomainEvent(@event);
    }

    private void Apply(RegulationEnactedEvent @event)
    {
        Id = @event.RegulationId;
        Title = @event.Title;
        Description = @event.Description;
        Type = new RegulationType(@event.TypeCode);
        CurrentVersion = new RegulationVersion(@event.Major, @event.Minor, @event.Patch);
        JurisdictionId = @event.JurisdictionId;
        IsActive = true;
        EnactedAt = @event.OccurredAt;
    }

    private void Apply(RegulationAmendedEvent @event)
    {
        Description = @event.Description;
        CurrentVersion = new RegulationVersion(@event.Major, @event.Minor, @event.Patch);
    }

    private void Apply(RegulationRevokedEvent @event)
    {
        IsActive = false;
        RevokedAt = @event.OccurredAt;
    }
}
