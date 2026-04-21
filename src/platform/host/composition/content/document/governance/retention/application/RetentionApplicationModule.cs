using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Content.Document.Governance.Retention;
using Whycespace.Shared.Contracts.Content.Document.Governance.Retention;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Content.Document.Governance.Retention.Application;

public static class RetentionApplicationModule
{
    public static IServiceCollection AddRetentionApplication(this IServiceCollection services)
    {
        services.AddTransient<ApplyRetentionHandler>();
        services.AddTransient<PlaceRetentionHoldHandler>();
        services.AddTransient<ReleaseRetentionHandler>();
        services.AddTransient<ExpireRetentionHandler>();
        services.AddTransient<MarkRetentionEligibleForDestructionHandler>();
        services.AddTransient<ArchiveRetentionHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<ApplyRetentionCommand, ApplyRetentionHandler>();
        engine.Register<PlaceRetentionHoldCommand, PlaceRetentionHoldHandler>();
        engine.Register<ReleaseRetentionCommand, ReleaseRetentionHandler>();
        engine.Register<ExpireRetentionCommand, ExpireRetentionHandler>();
        engine.Register<MarkRetentionEligibleForDestructionCommand, MarkRetentionEligibleForDestructionHandler>();
        engine.Register<ArchiveRetentionCommand, ArchiveRetentionHandler>();
    }
}
