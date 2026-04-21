using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Order.OrderCore.Reservation;
using Whycespace.Shared.Contracts.Business.Order.OrderCore.Reservation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Order.OrderCore.Reservation.Application;

public static class ReservationApplicationModule
{
    public static IServiceCollection AddReservationApplication(this IServiceCollection services)
    {
        services.AddTransient<HoldReservationHandler>();
        services.AddTransient<ConfirmReservationHandler>();
        services.AddTransient<ReleaseReservationHandler>();
        services.AddTransient<ExpireReservationHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<HoldReservationCommand, HoldReservationHandler>();
        engine.Register<ConfirmReservationCommand, ConfirmReservationHandler>();
        engine.Register<ReleaseReservationCommand, ReleaseReservationHandler>();
        engine.Register<ExpireReservationCommand, ExpireReservationHandler>();
    }
}
