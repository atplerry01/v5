using Whycespace.Domain.BusinessSystem.Order.OrderCore.Reservation;
using Whycespace.Shared.Contracts.Business.Order.OrderCore.Reservation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Order.OrderCore.Reservation;

public sealed class ConfirmReservationHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ConfirmReservationCommand cmd)
            return;

        var aggregate = (ReservationAggregate)await context.LoadAggregateAsync(typeof(ReservationAggregate));
        aggregate.Confirm(cmd.ConfirmedAt);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
