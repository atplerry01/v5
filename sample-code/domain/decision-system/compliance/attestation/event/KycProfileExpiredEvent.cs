namespace Whycespace.Domain.DecisionSystem.Compliance.Attestation;

using Whycespace.Domain.SharedKernel;

public sealed record KycProfileExpiredEvent(Guid ProfileId) : DomainEvent;
