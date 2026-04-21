using Whycespace.Domain.BusinessSystem.Order.OrderCore.Reservation;
using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Shared.Contracts.Business.Order.OrderCore.Reservation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Order.OrderCore.Reservation;

public sealed class HoldReservationHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not HoldReservationCommand cmd)
            return Task.CompletedTask;

        LineItemRef? lineItemRef = cmd.LineItemId is { } lid
            ? new LineItemRef(lid)
            : null;

        var aggregate = ReservationAggregate.Hold(
            new ReservationId(cmd.ReservationId),
            new OrderRef(cmd.OrderId),
            new ReservationSubjectRef(
                (ReservationSubjectKind)cmd.SubjectKind,
                new ReservationSubjectId(cmd.SubjectId)),
            new ReservationQuantity(cmd.QuantityValue, cmd.QuantityUnit),
            new ReservationExpiry(cmd.ExpiresAt),
            cmd.HeldAt,
            lineItemRef);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
