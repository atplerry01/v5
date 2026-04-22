using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.Audit.AuditTrace;
using Whycespace.Shared.Contracts.Control.Audit.AuditTrace;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.Audit.AuditTrace.Application;

public static class AuditTraceApplicationModule
{
    public static IServiceCollection AddAuditTraceApplication(this IServiceCollection services)
    {
        services.AddTransient<OpenAuditTraceHandler>();
        services.AddTransient<LinkAuditTraceEventHandler>();
        services.AddTransient<CloseAuditTraceHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<OpenAuditTraceCommand, OpenAuditTraceHandler>();
        engine.Register<LinkAuditTraceEventCommand, LinkAuditTraceEventHandler>();
        engine.Register<CloseAuditTraceCommand, CloseAuditTraceHandler>();
    }
}
