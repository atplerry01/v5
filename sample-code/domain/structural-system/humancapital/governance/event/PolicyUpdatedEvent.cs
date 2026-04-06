using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Governance;

public sealed record PolicyUpdatedEvent(
    Guid PolicyId,
    string NewRuleName
) : DomainEvent;
