using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T0U.Determinism;
using Whycespace.Engines.T0U.Determinism.Sequence;
using Whycespace.Engines.T0U.Determinism.Time;
using Whycespace.Platform.Host.Bootstrap;
using Whycespace.Runtime.Topology;
using Whycespace.Engines.T0U.WhyceChain.Engine;
using Whycespace.Engines.T0U.WhyceId.Engine;
using Whycespace.Engines.T0U.WhycePolicy.Engine;
using Whycespace.Shared.Kernel.Determinism;
using Whycespace.Engines.T1M.Core.Lifecycle;
using Whycespace.Engines.T1M.Core.StepExecutor;
using Whycespace.Engines.T1M.Core.WorkflowEngine;
using Whycespace.Runtime.ControlPlane;
using Whycespace.Runtime.Dispatcher;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Middleware;
using Whycespace.Runtime.Middleware.Execution;
using Whycespace.Runtime.Middleware.Observability;
using Whycespace.Runtime.Middleware.PostPolicy;
using Whycespace.Runtime.Middleware.PrePolicy;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Infrastructure.Admission;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Contracts.Infrastructure.Policy;
using Whycespace.Shared.Contracts.Policy;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;
using RuntimeMiddleware = Whycespace.Runtime.Middleware.IMiddleware;
using PolicyMw = Whycespace.Runtime.Middleware.Policy.PolicyMiddleware;

namespace Whycespace.Platform.Host.Composition.Runtime;

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

        // HSID v2.1 — parallel deterministic identity seam (see
        // claude/new-rules/20260407-200000-hsid-v2.1-parallel-seam.md).
        // Stateless engine + persisted sequence resolver (ISequenceStore is
        // registered by InfrastructureComposition). Topology resolver is
        // backed by an in-memory structure registry stub until the canonical
        // constitutional registry is wired in.
        services.AddSingleton<ITimeBucketProvider, DeterministicTimeBucketProvider>();
        services.AddSingleton<ISequenceResolver, PersistedSequenceResolver>();
        services.AddSingleton<IDeterministicIdEngine, DeterministicIdEngine>();
        services.AddSingleton<IStructureRegistry>(_ =>
            new InMemoryStructureRegistry(Array.Empty<StructureNode>()));
        services.AddSingleton<ITopologyResolver, TopologyResolver>();

        // HSID v2.1 H7 — fail-fast infrastructure validator. Resolved once
        // by Program.cs at host start; throws if hsid_sequences table is
        // missing or drifted (deterministic-id.guard.md G19/G20).
        services.AddSingleton<HsidInfrastructureValidator>();

        // Engines — T0U (identity + policy + chain, constitutional layer — all stateless)
        services.AddTransient<WhyceChainEngine>();
        services.AddTransient<WhyceIdEngine>();
        services.AddTransient(_ => new WhycePolicyEngine());

        // Policy decision event factory — engine-layer constructor for policy
        // domain events (runtime middleware cannot reference Whycespace.Domain.*).
        services.AddSingleton<IPolicyDecisionEventFactory, Whycespace.Engines.T0U.WhycePolicy.PolicyDecisionEventFactory>();

        // Payload type registry — populated by domain bootstrap modules,
        // consumed by WorkflowLifecycleEventFactory (write side stamps
        // PayloadType/OutputType discriminators) and by
        // WorkflowExecutionReplayService (read side rehydrates JsonElement
        // back into typed CLR objects). Domain-agnostic per
        // runtime.guard 11.R-DOM-01. See engine.guard E-TYPE-01..03.
        services.AddSingleton<IPayloadTypeRegistry>(sp =>
        {
            var registry = new PayloadTypeRegistry();
            foreach (var module in sp.GetServices<IDomainBootstrapModule>())
                module.RegisterPayloadTypes(registry);
            registry.Lock();
            return registry;
        });

        // Engines — T1M (workflow orchestration)
        services.AddTransient<WorkflowStepExecutor>();
        services.AddSingleton<WorkflowLifecycleEventFactory>();
        services.AddSingleton<IWorkflowEngine, T1MWorkflowEngine>();
        services.AddSingleton<IWorkflowExecutionReplayService, WorkflowExecutionReplayService>();

        // T1M workflow steps and T2E engines are registered by domain bootstrap modules.

        // --- Middleware Pipeline (LOCKED ORDER via RuntimeControlPlaneBuilder) ---
        services.AddSingleton<IReadOnlyList<RuntimeMiddleware>>(sp =>
        {
            var policyMiddleware = new PolicyMw(
                sp.GetRequiredService<WhyceIdEngine>(),
                sp.GetRequiredService<WhycePolicyEngine>(),
                sp.GetRequiredService<IPolicyEvaluator>(),
                sp.GetRequiredService<IIdGenerator>(),
                sp.GetRequiredService<IPolicyDecisionEventFactory>());

            var idempotencyMiddleware = new IdempotencyMiddleware(
                sp.GetRequiredService<IIdempotencyStore>());

            return new RuntimeControlPlaneBuilder()
                .UseTracing(new TracingMiddleware())
                .UseMetrics(new Whycespace.Runtime.Middleware.Observability.MetricsMiddleware())
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
            var registry = new Whycespace.Runtime.Workflow.WorkflowRegistry();
            foreach (var module in sp.GetServices<IDomainBootstrapModule>())
                module.RegisterWorkflows(registry);
            return registry;
        });

        // Workflow dispatcher — systems entry point for workflow execution
        services.AddSingleton<IWorkflowDispatcher, Whycespace.Systems.Midstream.Wss.WorkflowDispatcher>();

        // Runtime control plane — single entry point (uses EventFabric)
        services.AddSingleton<IRuntimeControlPlane>(sp =>
            new RuntimeControlPlane(
                sp.GetRequiredService<IReadOnlyList<RuntimeMiddleware>>(),
                sp.GetRequiredService<ICommandDispatcher>(),
                sp.GetRequiredService<IEventFabric>(),
                sp.GetRequiredService<IDeterministicIdEngine>(),
                sp.GetRequiredService<ISequenceResolver>(),
                sp.GetRequiredService<ITopologyResolver>(),
                sp.GetRequiredService<Whycespace.Shared.Contracts.Infrastructure.Health.IRuntimeStateAggregator>(),
                sp.GetRequiredService<Whycespace.Shared.Contracts.Infrastructure.Health.IRuntimeMaintenanceModeProvider>(),
                sp.GetRequiredService<Whycespace.Shared.Contracts.Runtime.IExecutionLockProvider>()));

        // phase1.5-S5.2.2 / KC-6 (WORKFLOW-ADMISSION-01): bind
        // WorkflowOptions from configuration with the record's
        // defaults as fallback. Register the WorkflowAdmissionGate
        // as a singleton so its partitioned limiters retain state
        // across requests; the dispatcher consumes it via
        // constructor injection. Conservative defaults
        // (PerWorkflowConcurrency=4, PerTenantConcurrency=6) sit
        // below the §5.2.2 KC-1 intake envelope so the gate refuses
        // before the intake limiter and the pool ever come close to
        // saturation.
        var workflowDefaults = new WorkflowOptions();
        var workflowOptions = new WorkflowOptions
        {
            PerWorkflowConcurrency = configuration.GetValue<int?>("Workflow:PerWorkflowConcurrency")
                ?? workflowDefaults.PerWorkflowConcurrency,
            PerTenantConcurrency = configuration.GetValue<int?>("Workflow:PerTenantConcurrency")
                ?? workflowDefaults.PerTenantConcurrency,
            QueueLimit = configuration.GetValue<int?>("Workflow:QueueLimit")
                ?? workflowDefaults.QueueLimit,
            RetryAfterSeconds = configuration.GetValue<int?>("Workflow:RetryAfterSeconds")
                ?? workflowDefaults.RetryAfterSeconds,
            // phase1.5-S5.2.3 / TC-7 (WORKFLOW-TIMEOUT-01): bind the
            // declared per-step and overall execution timeouts. Defaults
            // match the record so an absent configuration section is
            // still safe.
            PerStepTimeoutMs = configuration.GetValue<int?>("Workflow:PerStepTimeoutMs")
                ?? workflowDefaults.PerStepTimeoutMs,
            MaxExecutionMs = configuration.GetValue<int?>("Workflow:MaxExecutionMs")
                ?? workflowDefaults.MaxExecutionMs,
        };
        services.AddSingleton(workflowOptions);
        services.AddSingleton<WorkflowAdmissionGate>();

        // phase1.5-S5.2.2 / KW-1 (CHAIN-ANCHOR-DECLARED-01): bind
        // ChainAnchorOptions from configuration. The default permit
        // limit (1) is the only value the current chain integrity
        // invariant supports — KW-1 makes the declaration canonical
        // and audit-visible without changing behavior. Structural
        // restructuring of the lock is explicitly deferred.
        var chainAnchorDefaults = new ChainAnchorOptions();
        var chainAnchorOptions = new ChainAnchorOptions
        {
            PermitLimit = configuration.GetValue<int?>("ChainAnchor:PermitLimit")
                ?? chainAnchorDefaults.PermitLimit,
            // phase1.5-S5.2.3 / TC-2 (CHAIN-ANCHOR-WAIT-TIMEOUT-01):
            // bind the declared wait timeout and retry-after hint
            // alongside the existing PermitLimit. Defaults match the
            // record so an absent configuration section is still safe.
            WaitTimeoutMs = configuration.GetValue<int?>("ChainAnchor:WaitTimeoutMs")
                ?? chainAnchorDefaults.WaitTimeoutMs,
            RetryAfterSeconds = configuration.GetValue<int?>("ChainAnchor:RetryAfterSeconds")
                ?? chainAnchorDefaults.RetryAfterSeconds,
        };
        services.AddSingleton(chainAnchorOptions);

        // Command dispatcher (pure router) + system intent dispatcher
        services.AddSingleton<ICommandDispatcher, Whycespace.Runtime.Dispatcher.RuntimeCommandDispatcher>();
        services.AddSingleton<ISystemIntentDispatcher, Whycespace.Runtime.Dispatcher.SystemIntentDispatcher>();

        return services;
    }
}
