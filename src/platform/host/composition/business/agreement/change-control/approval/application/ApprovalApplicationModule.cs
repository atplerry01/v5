using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Business.Agreement.ChangeControl.Approval;
using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Approval;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Business.Agreement.ChangeControl.Approval.Application;

public static class ApprovalApplicationModule
{
    public static IServiceCollection AddApprovalApplication(this IServiceCollection services)
    {
        services.AddTransient<CreateApprovalHandler>();
        services.AddTransient<ApproveApprovalHandler>();
        services.AddTransient<RejectApprovalHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<CreateApprovalCommand, CreateApprovalHandler>();
        engine.Register<ApproveApprovalCommand, ApproveApprovalHandler>();
        engine.Register<RejectApprovalCommand, RejectApprovalHandler>();
    }
}
