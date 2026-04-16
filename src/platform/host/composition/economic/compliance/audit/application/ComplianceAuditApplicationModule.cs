using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Compliance.Audit;
using Whycespace.Shared.Contracts.Economic.Compliance.Audit;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Compliance.Audit.Application;

public static class ComplianceAuditApplicationModule
{
    public static IServiceCollection AddComplianceAuditApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateAuditRecordHandler>();
        services.AddTransient<FinalizeAuditRecordHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateAuditRecordCommand, CreateAuditRecordHandler>();
        engine.Register<FinalizeAuditRecordCommand, FinalizeAuditRecordHandler>();
    }
}
