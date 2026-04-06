using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Incentive;

public sealed record IncentiveRevokedEvent(
    Guid IncentiveId,
    string Reason
) : DomainEvent;
