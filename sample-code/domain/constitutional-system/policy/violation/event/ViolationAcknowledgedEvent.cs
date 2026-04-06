namespace Whycespace.Domain.ConstitutionalSystem.Policy.Violation;

using Whycespace.Domain.SharedKernel;

public sealed record ViolationAcknowledgedEvent(Guid ViolationId) : DomainEvent;
