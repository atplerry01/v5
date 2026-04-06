using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Operator;

public sealed record OperatorAuthorizedEvent(
    Guid OperatorId,
    string AuthorizationLevel
) : DomainEvent;
