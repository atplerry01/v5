using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Reserve;

public sealed class CanExpireSpecification : Specification<ReserveAggregate>
{
    private readonly Timestamp _currentTime;

    public CanExpireSpecification(Timestamp currentTime)
    {
        _currentTime = currentTime;
    }

    public override bool IsSatisfiedBy(ReserveAggregate reserve) =>
        reserve.Status == ReserveStatus.Active &&
        _currentTime.Value >= reserve.ExpiresAt.Value;
}
