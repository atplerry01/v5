using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Operator;

public sealed record OperatorRoleAssignedEvent(
    Guid OperatorId,
    Guid RoleId,
    string RoleName
) : DomainEvent;
