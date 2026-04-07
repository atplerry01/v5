using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whyce.Engines.T0U.WhyceChain.Engine;
using Whyce.Engines.T0U.WhyceId.Engine;
using Whyce.Engines.T0U.WhycePolicy.Engine;
using Whyce.Engines.T1M.StepExecutor;
using Whyce.Engines.T1M.WorkflowEngine;
using Whyce.Runtime.ControlPlane;
using Whyce.Runtime.Dispatcher;
using Whyce.Runtime.EventFabric;
using Whyce.Runtime.Middleware;
using Whyce.Runtime.Middleware.Execution;
using Whyce.Runtime.Middleware.Observability;
using Whyce.Runtime.Middleware.PostPolicy;
using Whyce.Runtime.Middleware.PrePolicy;
using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.Infrastructure.Persistence;
using Whyce.Shared.Contracts.Infrastructure.Policy;
using Whyce.Shared.Contracts.Runtime;
using RuntimeMiddleware = Whyce.Runtime.Middleware.IMiddleware;
using PolicyMw = Whyce.Runtime.Middleware.Policy.PolicyMiddleware;

namespace Whyce.Platform.Host.Composition.Runtime;

/// <summary>
/// Runtime control plane composition: T0U + T1M engines, registries populated by
/// domain bootstrap modules, the LOCKED middleware pipeline, dispatchers, and the
/// runtime control plane root. Middleware order is enforced by
/// <see cref="RuntimeControlPlaneBuilder"/> and the runtime-order guard.
/// </summary>
public static class RuntimeComposition
{
    public static IServiceCollection AddRuntimeComposition(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Engine registry — populated by domain bootstrap modules
        services.AddSingleton<IEngineRegistry>(sp =>
        {
            var registry = new EngineRegistry();
            foreach (var module in sp.GetServices<IDomainBootstrapModule>())
                module.RegisterEngines(registry);
            return registry;
        });

        // Engines — T0U (identity + policy + chain, constitutional layer — all stateless)
        services.AddTransient<WhyceChainEngine>();
        services.AddTransient<WhyceIdEngine>();
        services.AddTransient(_ => new WhycePolicyEngine());

        // Engines — T1M (workflow orchestration)
        services.AddTransient<WorkflowStepExecutor>();
        services.AddSingleton<IWorkflowEngine, T1MWorkflowEngine>();

        // T1M workflow steps and T2E engines are registered by domain bootstrap modules.

        // --- Middleware Pipeline (LOCKED ORDER via RuntimeControlPlaneBuilder) ---
        services.AddSingleton<IReadOnlyList<RuntimeMiddleware>>(sp =>
        {
            var policyMiddleware = new PolicyMw(
                sp.GetRequiredService<WhyceIdEngine>(),
                sp.GetRequiredService<WhycePolicyEngine>(),
                sp.GetRequiredService<IPolicyEvaluator>());

            var idempotencyMiddleware = new IdempotencyMiddleware(
                sp.GetRequiredService<IIdempotencyStore>());

            return new RuntimeControlPlaneBuilder()
                .UseTracing(new TracingMiddleware())
                .UseMetrics(new Whyce.Runtime.Middleware.Observability.MetricsMiddleware())
                .UseContextGuard(new ContextGuardMiddleware())
                .UseValidation(new ValidationMiddleware())
                .UsePolicy(policyMiddleware)
                .UseAuthorizationGuard(new AuthorizationGuardMiddleware())
                .UseIdempotency(idempotencyMiddleware)
                .UseExecutionGuard(new ExecutionGuardMiddleware())
                .Build();
        });

        // Workflow registry — populated by domain bootstrap modules
        services.AddSingleton<IWorkflowRegistry>(sp =>
        {
            var registry = new Whyce.Runtime.Workflow.WorkflowRegistry();
            foreach (var module in sp.GetServices<IDomainBootstrapModule>())
                module.RegisterWorkflows(registry);
            return registry;
        });

        // Workflow dispatcher — systems entry point for workflow execution
        services.AddSingleton<IWorkflowDispatcher, Whyce.Systems.Midstream.Wss.WorkflowDispatcher>();

        // Runtime control plane — single entry point (uses EventFabric)
        services.AddSingleton<IRuntimeControlPlane>(sp =>
            new RuntimeControlPlane(
                sp.GetRequiredService<IReadOnlyList<RuntimeMiddleware>>(),
                sp.GetRequiredService<ICommandDispatcher>(),
                sp.GetRequiredService<IEventFabric>()));

        // Command dispatcher (pure router) + system intent dispatcher
        services.AddSingleton<ICommandDispatcher, Whyce.Runtime.Dispatcher.RuntimeCommandDispatcher>();
        services.AddSingleton<ISystemIntentDispatcher, Whyce.Runtime.Dispatcher.SystemIntentDispatcher>();

        return services;
    }
}
