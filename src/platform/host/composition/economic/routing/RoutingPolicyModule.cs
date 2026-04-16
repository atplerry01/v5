using Microsoft.Extensions.DependencyInjection;
using Whycespace.Shared.Contracts.Economic.Routing.Execution;
using Whycespace.Shared.Contracts.Economic.Routing.Path;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Economic.Routing;

public static class RoutingPolicyModule
{
    public static IServiceCollection AddRoutingPolicyBindings(this IServiceCollection services)
    {
        // ── Routing / Path ─────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(DefineRoutingPathCommand),   RoutingPathPolicyIds.Define));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateRoutingPathCommand), RoutingPathPolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(DisableRoutingPathCommand),  RoutingPathPolicyIds.Disable));

        // ── Routing / Execution ────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(StartExecutionCommand),    RoutingExecutionPolicyIds.Start));
        services.AddSingleton(new CommandPolicyBinding(typeof(CompleteExecutionCommand), RoutingExecutionPolicyIds.Complete));
        services.AddSingleton(new CommandPolicyBinding(typeof(FailExecutionCommand),     RoutingExecutionPolicyIds.Fail));
        services.AddSingleton(new CommandPolicyBinding(typeof(AbortExecutionCommand),    RoutingExecutionPolicyIds.Abort));

        return services;
    }
}
