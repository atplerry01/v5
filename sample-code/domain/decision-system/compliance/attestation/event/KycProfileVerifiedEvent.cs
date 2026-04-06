namespace Whycespace.Domain.DecisionSystem.Compliance.Attestation;

using Whycespace.Domain.SharedKernel;

public sealed record KycProfileVerifiedEvent(
    Guid ProfileId,
    string RiskLevelValue,
    int ValidityDays) : DomainEvent;
