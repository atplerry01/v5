using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.Audit.AuditQuery;
using Whycespace.Shared.Contracts.Control.Audit.AuditQuery;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.Audit.AuditQuery.Application;

public static class AuditQueryApplicationModule
{
    public static IServiceCollection AddAuditQueryApplication(this IServiceCollection services)
    {
        services.AddTransient<IssueAuditQueryHandler>();
        services.AddTransient<CompleteAuditQueryHandler>();
        services.AddTransient<ExpireAuditQueryHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<IssueAuditQueryCommand, IssueAuditQueryHandler>();
        engine.Register<CompleteAuditQueryCommand, CompleteAuditQueryHandler>();
        engine.Register<ExpireAuditQueryCommand, ExpireAuditQueryHandler>();
    }
}
