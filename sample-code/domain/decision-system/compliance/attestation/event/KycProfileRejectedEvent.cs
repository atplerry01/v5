namespace Whycespace.Domain.DecisionSystem.Compliance.Attestation;

using Whycespace.Domain.SharedKernel;

public sealed record KycProfileRejectedEvent(
    Guid ProfileId,
    string Reason) : DomainEvent;
