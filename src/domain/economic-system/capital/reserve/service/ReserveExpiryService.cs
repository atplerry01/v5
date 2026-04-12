using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Reserve;

public sealed class ReserveExpiryService
{
    public bool TryExpire(ReserveAggregate reserve, Timestamp currentTime)
    {
        if (reserve.Status != ReserveStatus.Active)
            return false;

        if (currentTime.Value < reserve.ExpiresAt.Value)
            return false;

        reserve.Expire(currentTime);
        return true;
    }
}
