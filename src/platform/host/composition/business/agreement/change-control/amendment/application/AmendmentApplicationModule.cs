using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Agreement.ChangeControl.Amendment;
using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Amendment;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Agreement.ChangeControl.Amendment.Application;

public static class AmendmentApplicationModule
{
    public static IServiceCollection AddAmendmentApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateAmendmentHandler>();
        services.AddTransient<ApplyAmendmentHandler>();
        services.AddTransient<RevertAmendmentHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateAmendmentCommand, CreateAmendmentHandler>();
        engine.Register<ApplyAmendmentCommand, ApplyAmendmentHandler>();
        engine.Register<RevertAmendmentCommand, RevertAmendmentHandler>();
    }
}
