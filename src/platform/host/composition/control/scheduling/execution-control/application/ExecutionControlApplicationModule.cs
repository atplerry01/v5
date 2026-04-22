using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Control.Scheduling.ExecutionControl;
using Whycespace.Shared.Contracts.Control.Scheduling.ExecutionControl;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Control.Scheduling.ExecutionControl.Application;

public static class ExecutionControlApplicationModule
{
    public static IServiceCollection AddExecutionControlApplication(this IServiceCollection services)
    {
        services.AddTransient<IssueExecutionControlHandler>();
        services.AddTransient<RecordExecutionControlOutcomeHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<IssueExecutionControlCommand, IssueExecutionControlHandler>();
        engine.Register<RecordExecutionControlOutcomeCommand, RecordExecutionControlOutcomeHandler>();
    }
}
