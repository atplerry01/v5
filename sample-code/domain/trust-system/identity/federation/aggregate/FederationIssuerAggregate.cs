using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.TrustSystem.Identity.Federation;

/// <summary>
/// An external identity provider registered in the federation trust registry.
/// Lifecycle: Pending → Approved → (Suspended ↔ Approved) → Revoked.
/// </summary>
public sealed class FederationIssuerAggregate : AggregateRoot
{
    public IssuerId IssuerId { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public IssuerType IssuerType { get; private set; } = null!;
    public TrustLevel TrustLevel { get; private set; } = null!;
    public IssuerStatus Status { get; private set; } = null!;
    public DateTimeOffset? ApprovedAt { get; private set; }
    public IReadOnlyDictionary<string, string> Metadata => _metadata;

    private readonly Dictionary<string, string> _metadata = new();

    private FederationIssuerAggregate() { }

    public static FederationIssuerAggregate Register(
        string name,
        IssuerType issuerType,
        TrustLevel initialTrust,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        Guard.AgainstEmpty(name);
        Guard.AgainstNull(issuerType);
        Guard.AgainstNull(initialTrust);

        var issuer = new FederationIssuerAggregate
        {
            IssuerId = IssuerId.FromSeed($"FederationIssuer:{name}:{issuerType.Value}"),
            Name = name,
            IssuerType = issuerType,
            TrustLevel = initialTrust,
            Status = IssuerStatus.Pending
        };
        issuer.Id = issuer.IssuerId.Value;

        if (metadata is not null)
        {
            foreach (var kvp in metadata)
                issuer._metadata[kvp.Key] = kvp.Value;
        }

        issuer.RaiseDomainEvent(new IssuerRegisteredEvent(
            issuer.IssuerId.Value, name, issuerType.Value));

        return issuer;
    }

    public void Approve(DateTimeOffset approvedAt)
    {
        EnsureInvariant(
            Status == IssuerStatus.Pending || Status == IssuerStatus.Suspended,
            "APPROVABLE_STATUS",
            $"Cannot approve issuer in '{Status}' status. Must be Pending or Suspended.");

        EnsureInvariant(
            Status != IssuerStatus.Approved,
            "NOT_ALREADY_APPROVED",
            "Issuer is already approved.");

        Status = IssuerStatus.Approved;
        ApprovedAt = approvedAt;

        RaiseDomainEvent(new IssuerApprovedEvent(IssuerId.Value, approvedAt));
    }

    public void Suspend(string reason)
    {
        Guard.AgainstEmpty(reason);
        EnsureInvariant(
            Status == IssuerStatus.Approved,
            "MUST_BE_APPROVED",
            "Can only suspend an approved issuer.");

        Status = IssuerStatus.Suspended;
        RaiseDomainEvent(new IssuerSuspendedEvent(IssuerId.Value, reason));
    }

    public void Revoke(string reason)
    {
        Guard.AgainstEmpty(reason);
        EnsureNotTerminal(Status, s => s == IssuerStatus.Revoked, "Revoke");

        Status = IssuerStatus.Revoked;
        RaiseDomainEvent(new IssuerRevokedEvent(IssuerId.Value, reason));
    }

    public bool IsApproved => Status == IssuerStatus.Approved;

    public void UpdateTrustLevel(TrustLevel newLevel)
    {
        Guard.AgainstNull(newLevel);
        TrustLevel = newLevel;
    }
}

/// <summary>
/// Explicit issuer lifecycle states.
/// </summary>
public sealed record IssuerStatus
{
    public string Value { get; }
    private IssuerStatus(string value) => Value = value;

    public static readonly IssuerStatus Pending = new("Pending");
    public static readonly IssuerStatus Approved = new("Approved");
    public static readonly IssuerStatus Suspended = new("Suspended");
    public static readonly IssuerStatus Revoked = new("Revoked");

    public override string ToString() => Value;
}
