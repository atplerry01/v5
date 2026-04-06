namespace Whycespace.Domain.ConstitutionalSystem.Policy.Violation;

using Whycespace.Domain.SharedKernel;

public sealed record ViolationResolvedEvent(
    Guid ViolationId,
    string ResolutionNote) : DomainEvent;
