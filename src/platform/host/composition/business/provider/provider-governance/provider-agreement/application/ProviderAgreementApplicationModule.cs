using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Provider.ProviderGovernance.ProviderAgreement;
using Whycespace.Shared.Contracts.Business.Provider.ProviderGovernance.ProviderAgreement;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Provider.ProviderGovernance.ProviderAgreement.Application;

public static class ProviderAgreementApplicationModule
{
    public static IServiceCollection AddProviderAgreementApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateProviderAgreementHandler>();
        services.AddTransient<ActivateProviderAgreementHandler>();
        services.AddTransient<SuspendProviderAgreementHandler>();
        services.AddTransient<TerminateProviderAgreementHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateProviderAgreementCommand, CreateProviderAgreementHandler>();
        engine.Register<ActivateProviderAgreementCommand, ActivateProviderAgreementHandler>();
        engine.Register<SuspendProviderAgreementCommand, SuspendProviderAgreementHandler>();
        engine.Register<TerminateProviderAgreementCommand, TerminateProviderAgreementHandler>();
    }
}
