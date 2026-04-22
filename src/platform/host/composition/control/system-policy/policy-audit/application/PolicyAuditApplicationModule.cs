using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.SystemPolicy.PolicyAudit;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyAudit;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.SystemPolicy.PolicyAudit.Application;

public static class PolicyAuditApplicationModule
{
    public static IServiceCollection AddPolicyAuditApplication(this IServiceCollection services)
    {
        services.AddTransient<RecordPolicyAuditEntryHandler>();
        services.AddTransient<ReviewPolicyAuditEntryHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RecordPolicyAuditEntryCommand, RecordPolicyAuditEntryHandler>();
        engine.Register<ReviewPolicyAuditEntryCommand, ReviewPolicyAuditEntryHandler>();
    }
}
