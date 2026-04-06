using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Incentive;

public sealed record IncentiveGrantedEvent(
    Guid IncentiveId,
    string IncentiveType,
    decimal Amount,
    string Currency
) : DomainEvent;
