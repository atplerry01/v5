using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.Scheduling.ScheduleControl;
using Whycespace.Shared.Contracts.Control.Scheduling.ScheduleControl;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.Scheduling.ScheduleControl.Application;

public static class ScheduleControlApplicationModule
{
    public static IServiceCollection AddScheduleControlApplication(this IServiceCollection services)
    {
        services.AddTransient<DefineScheduleControlHandler>();
        services.AddTransient<SuspendScheduleControlHandler>();
        services.AddTransient<ResumeScheduleControlHandler>();
        services.AddTransient<RetireScheduleControlHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<DefineScheduleControlCommand, DefineScheduleControlHandler>();
        engine.Register<SuspendScheduleControlCommand, SuspendScheduleControlHandler>();
        engine.Register<ResumeScheduleControlCommand, ResumeScheduleControlHandler>();
        engine.Register<RetireScheduleControlCommand, RetireScheduleControlHandler>();
    }
}
