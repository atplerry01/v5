using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Agreement.Commitment.Validity;
using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Validity;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Agreement.Commitment.Validity.Application;

public static class ValidityApplicationModule
{
    public static IServiceCollection AddValidityApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateValidityHandler>();
        services.AddTransient<ExpireValidityHandler>();
        services.AddTransient<InvalidateValidityHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateValidityCommand, CreateValidityHandler>();
        engine.Register<ExpireValidityCommand, ExpireValidityHandler>();
        engine.Register<InvalidateValidityCommand, InvalidateValidityHandler>();
    }
}
