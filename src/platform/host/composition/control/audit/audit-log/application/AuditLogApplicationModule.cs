using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.Audit.AuditLog;
using Whycespace.Shared.Contracts.Control.Audit.AuditLog;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.Audit.AuditLog.Application;

public static class AuditLogApplicationModule
{
    public static IServiceCollection AddAuditLogApplication(this IServiceCollection services)
    {
        services.AddTransient<RecordAuditLogEntryHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RecordAuditLogEntryCommand, RecordAuditLogEntryHandler>();
    }
}
