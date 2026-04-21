using Whycespace.Domain.BusinessSystem.Order.OrderCore.Reservation;
using Whycespace.Shared.Contracts.Business.Order.OrderCore.Reservation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Order.OrderCore.Reservation;

public sealed class ReleaseReservationHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ReleaseReservationCommand cmd)
            return;

        var aggregate = (ReservationAggregate)await context.LoadAggregateAsync(typeof(ReservationAggregate));
        aggregate.Release(cmd.ReleasedAt);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
