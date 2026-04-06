namespace Whycespace.Domain.DecisionSystem.Compliance.Attestation;

using Whycespace.Domain.DecisionSystem.Compliance.Attestation;
using Whycespace.Domain.SharedKernel;

public sealed class KycProfileAggregate : AggregateRoot
{
    public Guid IdentityId { get; private set; }
    public Guid JurisdictionId { get; private set; }
    public VerificationStatus Status { get; private set; } = default!;
    public RiskLevel Risk { get; private set; } = default!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? VerifiedAt { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }
    public string? RejectionReason { get; private set; }

    private KycProfileAggregate() { }

    public static KycProfileAggregate Create(Guid profileId, Guid identityId, Guid jurisdictionId)
    {
        var profile = new KycProfileAggregate();
        var @event = new KycProfileCreatedEvent(profileId, identityId, jurisdictionId);
        profile.Apply(@event);
        profile.RaiseDomainEvent(@event);
        return profile;
    }

    public void SubmitForVerification()
    {
        if (Status != VerificationStatus.Pending && Status != VerificationStatus.Rejected)
            throw new DomainException(KycErrors.InvalidTransition, $"Cannot submit profile in '{Status.Value}' status.");

        var @event = new KycVerificationSubmittedEvent(Id);
        Apply(@event);
        RaiseDomainEvent(@event);
    }

    public void Verify(RiskLevel riskLevel, int validityDays)
    {
        if (Status != VerificationStatus.UnderReview)
            throw new DomainException(KycErrors.InvalidTransition, "Only profiles under review can be verified.");

        var @event = new KycProfileVerifiedEvent(Id, riskLevel.Value, validityDays);
        Apply(@event);
        RaiseDomainEvent(@event);
    }

    public void Reject(string reason)
    {
        if (Status != VerificationStatus.UnderReview)
            throw new DomainException(KycErrors.InvalidTransition, "Only profiles under review can be rejected.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException(KycErrors.InvalidRejectionReason, "Rejection reason is required.");

        var @event = new KycProfileRejectedEvent(Id, reason);
        Apply(@event);
        RaiseDomainEvent(@event);
    }

    public void Suspend(string reason)
    {
        if (Status != VerificationStatus.Verified)
            throw new DomainException(KycErrors.InvalidTransition, "Only verified profiles can be suspended.");

        var @event = new KycProfileSuspendedEvent(Id, reason);
        Apply(@event);
        RaiseDomainEvent(@event);
    }

    public void Expire()
    {
        if (Status != VerificationStatus.Verified)
            throw new DomainException(KycErrors.InvalidTransition, "Only verified profiles can expire.");

        var @event = new KycProfileExpiredEvent(Id);
        Apply(@event);
        RaiseDomainEvent(@event);
    }

    public void Renew(RiskLevel riskLevel, int validityDays)
    {
        if (Status != VerificationStatus.Expired && Status != VerificationStatus.Verified)
            throw new DomainException(KycErrors.InvalidTransition, $"Cannot renew profile in '{Status.Value}' status.");

        var @event = new KycProfileRenewedEvent(Id, riskLevel.Value, validityDays);
        Apply(@event);
        RaiseDomainEvent(@event);
    }

    public bool IsExpired(DateTimeOffset asOf) =>
        ExpiresAt.HasValue && asOf > ExpiresAt.Value;

    public bool RequiresEnhancedDueDiligence =>
        Risk == RiskLevel.High || Risk == RiskLevel.Critical;

    private void Apply(KycProfileCreatedEvent @event)
    {
        Id = @event.ProfileId;
        IdentityId = @event.IdentityId;
        JurisdictionId = @event.JurisdictionId;
        Status = VerificationStatus.Pending;
        Risk = RiskLevel.Unknown;
        CreatedAt = @event.OccurredAt;
    }

    private void Apply(KycVerificationSubmittedEvent _)
    {
        Status = VerificationStatus.UnderReview;
    }

    private void Apply(KycProfileVerifiedEvent @event)
    {
        Status = VerificationStatus.Verified;
        Risk = new RiskLevel(@event.RiskLevelValue);
        VerifiedAt = @event.OccurredAt;
        ExpiresAt = @event.OccurredAt.AddDays(@event.ValidityDays);
    }

    private void Apply(KycProfileRejectedEvent @event)
    {
        Status = VerificationStatus.Rejected;
        RejectionReason = @event.Reason;
    }

    private void Apply(KycProfileSuspendedEvent _)
    {
        Status = VerificationStatus.Suspended;
    }

    private void Apply(KycProfileExpiredEvent _)
    {
        Status = VerificationStatus.Expired;
    }

    private void Apply(KycProfileRenewedEvent @event)
    {
        Status = VerificationStatus.Verified;
        Risk = new RiskLevel(@event.RiskLevelValue);
        VerifiedAt = @event.OccurredAt;
        ExpiresAt = @event.OccurredAt.AddDays(@event.ValidityDays);
    }
}
