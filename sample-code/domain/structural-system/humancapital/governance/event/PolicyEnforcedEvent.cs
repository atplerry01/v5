using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Governance;

public sealed record PolicyEnforcedEvent(
    Guid PolicyId
) : DomainEvent;
