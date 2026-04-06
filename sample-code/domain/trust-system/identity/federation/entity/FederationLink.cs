using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.TrustSystem.Identity.Federation;

/// <summary>
/// A link between a local WhyceID identity and an external identity at a federation issuer.
/// Tracks confidence evolution and link provenance.
/// </summary>
public sealed class FederationLink : Entity
{
    public ExternalIdentityId ExternalId { get; private set; } = null!;
    public IssuerId IssuerId { get; private set; } = null!;
    public DateTimeOffset LinkedAt { get; private set; }
    public ConfidenceProfile ConfidenceProfile { get; private set; } = null!;
    public VerificationLevel Verification { get; private set; } = null!;
    public LinkProvenance Provenance { get; private set; } = null!;
    public FederationLinkStatus Status { get; private set; } = null!;

    private FederationLink() { }

    public static FederationLink Create(
        ExternalIdentityId externalId,
        IssuerId issuerId,
        ConfidenceLevel initialConfidence,
        VerificationLevel verification,
        LinkProvenance provenance,
        DateTimeOffset linkedAt)
    {
        Guard.AgainstNull(externalId);
        Guard.AgainstNull(issuerId);
        Guard.AgainstNull(initialConfidence);
        Guard.AgainstNull(verification);
        Guard.AgainstNull(provenance);

        return new FederationLink
        {
            Id = DeterministicIdHelper.FromSeed($"FederationLink:{externalId.Value}:{issuerId.Value}"),
            ExternalId = externalId,
            IssuerId = issuerId,
            LinkedAt = linkedAt,
            ConfidenceProfile = new ConfidenceProfile(initialConfidence, linkedAt),
            Verification = verification,
            Provenance = provenance,
            Status = FederationLinkStatus.Active
        };
    }

    /// <summary>
    /// Boost confidence (e.g. credential verified successfully).
    /// </summary>
    public void BoostConfidence(decimal amount, string reason, DateTimeOffset at)
    {
        if (Status != FederationLinkStatus.Active)
            throw new InvalidOperationException("Can only adjust confidence on an active link.");
        ConfidenceProfile.IncreaseConfidence(amount, reason, at);
    }

    /// <summary>
    /// Reduce confidence (e.g. anomaly detected).
    /// </summary>
    public void ReduceConfidence(decimal amount, string reason, DateTimeOffset at)
    {
        if (Status != FederationLinkStatus.Active)
            throw new InvalidOperationException("Can only adjust confidence on an active link.");
        ConfidenceProfile.DecreaseConfidence(amount, reason, at);
    }

    public void Suspend()
    {
        if (Status != FederationLinkStatus.Active)
            throw new InvalidOperationException("Can only suspend an active link.");
        Status = FederationLinkStatus.Suspended;
    }

    public void Revoke()
    {
        if (Status == FederationLinkStatus.Revoked)
            throw new InvalidOperationException("Link is already revoked.");
        Status = FederationLinkStatus.Revoked;
    }

    public void Reactivate()
    {
        if (Status != FederationLinkStatus.Suspended)
            throw new InvalidOperationException("Can only reactivate a suspended link.");
        Status = FederationLinkStatus.Active;
    }
}
