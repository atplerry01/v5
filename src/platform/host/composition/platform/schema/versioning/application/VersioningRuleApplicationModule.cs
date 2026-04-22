using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Platform.Schema.Versioning;
using Whycespace.Shared.Contracts.Platform.Schema.Versioning;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Platform.Schema.Versioning.Application;

public static class VersioningRuleApplicationModule
{
    public static IServiceCollection AddVersioningRuleApplication(this IServiceCollection services)
    {
        services.AddTransient<RegisterVersioningRuleHandler>();
        services.AddTransient<IssueVersioningVerdictHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RegisterVersioningRuleCommand, RegisterVersioningRuleHandler>();
        engine.Register<IssueVersioningVerdictCommand, IssueVersioningVerdictHandler>();
    }
}
