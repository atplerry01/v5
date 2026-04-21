using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Provider.ProviderScope.ProviderCoverage;
using Whycespace.Shared.Contracts.Business.Provider.ProviderScope.ProviderCoverage;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Provider.ProviderScope.ProviderCoverage.Application;

public static class ProviderCoverageApplicationModule
{
    public static IServiceCollection AddProviderCoverageApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateProviderCoverageHandler>();
        services.AddTransient<AddCoverageScopeHandler>();
        services.AddTransient<RemoveCoverageScopeHandler>();
        services.AddTransient<ActivateProviderCoverageHandler>();
        services.AddTransient<ArchiveProviderCoverageHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateProviderCoverageCommand, CreateProviderCoverageHandler>();
        engine.Register<AddCoverageScopeCommand, AddCoverageScopeHandler>();
        engine.Register<RemoveCoverageScopeCommand, RemoveCoverageScopeHandler>();
        engine.Register<ActivateProviderCoverageCommand, ActivateProviderCoverageHandler>();
        engine.Register<ArchiveProviderCoverageCommand, ArchiveProviderCoverageHandler>();
    }
}
