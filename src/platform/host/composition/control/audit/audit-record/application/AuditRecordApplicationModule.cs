using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.Audit.AuditRecord;
using Whycespace.Shared.Contracts.Control.Audit.AuditRecord;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.Audit.AuditRecord.Application;

public static class AuditRecordApplicationModule
{
    public static IServiceCollection AddAuditRecordApplication(this IServiceCollection services)
    {
        services.AddTransient<RaiseAuditRecordHandler>();
        services.AddTransient<ResolveAuditRecordHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RaiseAuditRecordCommand, RaiseAuditRecordHandler>();
        engine.Register<ResolveAuditRecordCommand, ResolveAuditRecordHandler>();
    }
}
