using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Governance;

public sealed record PolicyCreatedEvent(
    Guid PolicyId,
    string RuleName
) : DomainEvent;
