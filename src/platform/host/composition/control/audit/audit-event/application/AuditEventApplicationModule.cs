using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.Audit.AuditEvent;
using Whycespace.Shared.Contracts.Control.Audit.AuditEvent;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.Audit.AuditEvent.Application;

public static class AuditEventApplicationModule
{
    public static IServiceCollection AddAuditEventApplication(this IServiceCollection services)
    {
        services.AddTransient<CaptureAuditEventHandler>();
        services.AddTransient<SealAuditEventHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CaptureAuditEventCommand, CaptureAuditEventHandler>();
        engine.Register<SealAuditEventCommand, SealAuditEventHandler>();
    }
}
