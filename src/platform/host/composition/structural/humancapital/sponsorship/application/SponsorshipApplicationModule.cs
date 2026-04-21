using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Structural.Humancapital.Sponsorship;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Humancapital.Sponsorship;

namespace Whycespace.Platform.Host.Composition.Structural.Humancapital.Sponsorship.Application;

public static class SponsorshipApplicationModule
{
    public static IServiceCollection AddSponsorshipApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateSponsorshipHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateSponsorshipCommand, CreateSponsorshipHandler>();
    }
}
