namespace Whycespace.Domain.ConstitutionalSystem.Policy.Violation;

using Whycespace.Domain.SharedKernel;

public sealed record ViolationEscalatedEvent(
    Guid ViolationId,
    string Severity) : DomainEvent;
