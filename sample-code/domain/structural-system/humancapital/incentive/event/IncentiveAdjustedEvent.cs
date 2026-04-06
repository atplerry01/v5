using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Incentive;

public sealed record IncentiveAdjustedEvent(
    Guid IncentiveId,
    decimal NewAmount
) : DomainEvent;
