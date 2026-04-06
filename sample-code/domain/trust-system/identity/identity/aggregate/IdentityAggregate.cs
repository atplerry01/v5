namespace Whycespace.Domain.TrustSystem.Identity.Identity;

public sealed class IdentityAggregate : AggregateRoot
{
    public IdentityId IdentityId { get; private set; } = null!;
    public IdentityType IdentityType { get; private set; } = null!;
    public IdentityStatus Status { get; private set; } = null!;
    public string DisplayName { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ActivatedAt { get; private set; }
    public DateTimeOffset? DeactivatedAt { get; private set; }

    private IdentityAggregate() { }

    public static IdentityAggregate Register(
        IdentityId identityId,
        IdentityType identityType,
        string displayName,
        DateTimeOffset timestamp)
    {
        Guard.AgainstNull(identityId);
        Guard.AgainstNull(identityType);
        Guard.AgainstEmpty(displayName);

        var aggregate = new IdentityAggregate
        {
            IdentityId = identityId,
            IdentityType = identityType,
            Status = IdentityStatus.Pending,
            DisplayName = displayName,
            CreatedAt = timestamp
        };

        aggregate.Id = identityId.Value;

        aggregate.RaiseDomainEvent(new IdentityRegisteredEvent(
            identityId.Value,
            identityType.Value,
            displayName));

        return aggregate;
    }

    public void Activate(DateTimeOffset timestamp)
    {
        EnsureInvariant(
            Status == IdentityStatus.Pending,
            "IDENTITY_MUST_BE_PENDING",
            $"Cannot activate identity in '{Status}' state.");

        Status = IdentityStatus.Active;
        ActivatedAt = timestamp;

        RaiseDomainEvent(new IdentityActivatedEvent(IdentityId.Value));
    }

    public void Suspend(string reason)
    {
        Guard.AgainstEmpty(reason);
        EnsureInvariant(
            Status == IdentityStatus.Active,
            "IDENTITY_MUST_BE_ACTIVE",
            "Cannot suspend identity that is not active.");

        Status = IdentityStatus.Suspended;

        RaiseDomainEvent(new IdentitySuspendedEvent(IdentityId.Value, reason));
    }

    public void Reactivate()
    {
        EnsureInvariant(
            Status == IdentityStatus.Suspended,
            "IDENTITY_MUST_BE_SUSPENDED",
            "Cannot reactivate identity that is not suspended.");

        Status = IdentityStatus.Active;

        RaiseDomainEvent(new IdentityReactivatedEvent(IdentityId.Value));
    }

    public void Deactivate(string reason, DateTimeOffset timestamp)
    {
        Guard.AgainstEmpty(reason);
        EnsureNotTerminal(Status, s => s == IdentityStatus.Deactivated, "Deactivate");

        Status = IdentityStatus.Deactivated;
        DeactivatedAt = timestamp;

        RaiseDomainEvent(new IdentityDeactivatedEvent(IdentityId.Value, reason));
    }

    public void UpdateDisplayName(string newDisplayName)
    {
        Guard.AgainstEmpty(newDisplayName);
        EnsureNotTerminal(Status, s => s == IdentityStatus.Deactivated, "UpdateDisplayName");

        var oldName = DisplayName;
        DisplayName = newDisplayName;

        RaiseDomainEvent(new IdentityProfileUpdatedEvent(IdentityId.Value, oldName, newDisplayName));
    }
}
