using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Operator;

public sealed record OperatorRevokedEvent(
    Guid OperatorId,
    string Reason
) : DomainEvent;
