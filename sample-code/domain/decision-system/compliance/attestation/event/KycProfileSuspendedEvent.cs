namespace Whycespace.Domain.DecisionSystem.Compliance.Attestation;

using Whycespace.Domain.SharedKernel;

public sealed record KycProfileSuspendedEvent(
    Guid ProfileId,
    string Reason) : DomainEvent;
