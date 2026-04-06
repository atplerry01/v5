using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Whycespace.Runtime.Chain;
using Whycespace.Runtime.ControlPlane;
using Whycespace.Runtime.ControlPlane.Middleware;
using Whycespace.Runtime.Dispatcher;
using Whycespace.Runtime.Engine;
using Whycespace.Runtime.Engine.Domain.Economic.Ledger;
using Whycespace.Runtime.Engine.Domain.Economic.Transaction;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Idempotency;
using Whycespace.Runtime.Observability;
using Whycespace.Runtime.Retry.PolicyAnchor;
using Whycespace.Runtime.Routing;
using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Domain.Economic.Ledger;
using Whycespace.Shared.Contracts.Domain.Economic.Transaction;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Policy;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.Bootstrap;

/// <summary>
/// Bootstraps the RuntimeControlPlane with all mandatory middleware,
/// engine registrations, and event publishing infrastructure.
///
/// Engine-agnostic: does NOT reference engine types. Engine registration
/// is delegated to the composition root (FoundationHost) via EngineResolver
/// pre-population and workflow/route registration callbacks.
///
/// Policy-strict: IPolicyEvaluator and IPolicyDecisionAnchor MUST be
/// registered by the composition root. No bypass. No pass-through defaults.
/// </summary>
public static class RuntimeBootstrap
{
    public static readonly DomainRoute IncidentRoute = new()
    {
        Cluster = "operational",
        Context = "incident"
    };

    public static readonly DomainRoute TodoRoute = new()
    {
        Cluster = "operational",
        Context = "todo"
    };

    public static readonly DomainRoute PolicyRoute = new()
    {
        Cluster = "governance",
        Context = "policy"
    };

    /// <summary>
    /// Registers runtime infrastructure services in the DI container.
    ///
    /// REQUIRES (must be pre-registered by composition root):
    ///   - IPolicyEvaluator (WHYCEPOLICY — real OPA evaluator)
    ///   - IPolicyDecisionAnchor (WhyceChain — real chain anchor)
    ///   - IAggregateStore
    ///   - IClock
    /// </summary>
    public static void RegisterRuntimeServices(IServiceCollection services)
    {
        // ── Observability infrastructure ──
        var sharedMetricsCollector = new MetricsCollector();
        services.AddSingleton(sharedMetricsCollector);
        services.AddSingleton(new EnforcementMetrics(sharedMetricsCollector));
        services.AddSingleton(sp =>
            new EnforcementAnomalyEmitter(
                sp.GetRequiredService<MetricsCollector>(),
                sp.GetRequiredService<IClock>(),
                NullLogger<EnforcementAnomalyEmitter>.Instance,
                sp.GetService<IEventPublisher>()));

        // ── Policy anchor retry queue ──
        services.AddSingleton(sp =>
            new PolicyAnchorRetryQueue(sp.GetRequiredService<IClock>()));

        // ── Economic domain services ──
        // These resolve IPolicyEvaluator and IPolicyDecisionAnchor from DI.
        // The composition root MUST register real implementations before these resolve.
        services.AddSingleton<ILedgerDomainService>(sp =>
            new LedgerDomainService(
                sp.GetRequiredService<IAggregateStore>(),
                sp.GetRequiredService<IPolicyEvaluator>(),
                sp.GetRequiredService<IPolicyDecisionAnchor>(),
                sp.GetRequiredService<EnforcementMetrics>(),
                sp.GetRequiredService<EnforcementAnomalyEmitter>(),
                sp.GetRequiredService<IClock>(),
                sp.GetRequiredService<IIdGenerator>()));

        services.AddSingleton<ITransactionDomainService>(sp =>
            new TransactionDomainService(
                sp.GetRequiredService<IAggregateStore>(),
                sp.GetRequiredService<IPolicyEvaluator>(),
                sp.GetRequiredService<IPolicyDecisionAnchor>(),
                sp.GetRequiredService<EnforcementMetrics>(),
                sp.GetRequiredService<EnforcementAnomalyEmitter>(),
                sp.GetRequiredService<IClock>()));
    }

    /// <summary>
    /// Builds the control plane with real WHYCEPOLICY enforcement.
    ///
    /// REQUIRES:
    ///   - policyInvoker: Real IPolicyEngineInvoker (OPA-backed, NOT a bypass)
    ///   - Engines pre-registered in the provided EngineResolver
    ///   - Domain routes pre-registered in the provided DomainRouteResolver
    /// </summary>
    public static IRuntimeControlPlane Build(
        IEventPublisher eventPublisher,
        DomainRouteResolver routeResolver,
        EngineResolver engineResolver,
        IPolicyEngineInvoker policyInvoker,
        Action<WorkflowResolver>? registerWorkflows = null)
    {
        ArgumentNullException.ThrowIfNull(policyInvoker,
            "IPolicyEngineInvoker is REQUIRED. No policy bypass allowed.");

        var builder = new RuntimeControlPlaneBuilder();

        // ── Mandatory middleware — REAL policy enforcement ──
        builder.UseValidation(new ValidationMiddleware());
        builder.UseIdempotency(new IdempotencyMiddleware(new InMemoryIdempotencyRegistry()));
        builder.UseAuthorization(new AuthorizationMiddleware(new AllowAllAuthorizationProvider()));
        builder.UsePolicy(new PolicyMiddleware(policyInvoker));
        builder.UseExecutionGuard(new ExecutionGuardMiddleware(new AllowAllExecutionGuard()));

        // ── Event publishing ──
        builder.UseEventPublisher(eventPublisher);

        // ── Domain route registration (non-economic) ──
        RegisterDomainRoutes(routeResolver);

        // ── Engine registration from pre-populated registry ──
        foreach (var descriptor in engineResolver.GetAll())
        {
            builder.Engines.Register(descriptor);
        }

        // ── Workflow registration: core lifecycle workflows ──
        RegisterIncidentWorkflow(builder, routeResolver);
        RegisterTodoWorkflow(builder, routeResolver);
        RegisterPolicyWorkflow(builder, routeResolver);

        // ── External workflow registration (economic, etc.) ──
        registerWorkflows?.Invoke(builder.Workflows);

        var controlPlane = builder.Build(routeResolver);
        return new RuntimeControlPlaneAdapter(controlPlane);
    }

    private static void RegisterDomainRoutes(DomainRouteResolver routeResolver)
    {
        routeResolver.Register(IncidentRoute,
            "create", "assign", "escalate", "resolve", "close");

        routeResolver.Register(TodoRoute,
            "create", "complete");

        routeResolver.Register(PolicyRoute,
            "proposal.submit", "approve", "activate",
            "get", "versions", "active", "history",
            "governance.pending", "governance.approvals",
            "simulate", "simulate.batch",
            "federation.build", "federation.evaluate", "federation.get");
    }

    private static void RegisterTodoWorkflow(
        RuntimeControlPlaneBuilder builder,
        DomainRouteResolver routeResolver)
    {
        var route = TodoRoute;
        var actions = new[] { "create", "complete" };

        foreach (var action in actions)
        {
            var commandType = route.ResolveCommandType(action);
            builder.Workflows.Register(
                commandType,
                _ => Task.FromResult(new WorkflowStep[]
                {
                    new() { EngineCommandType = commandType }
                }));
        }
    }

    private static void RegisterPolicyWorkflow(
        RuntimeControlPlaneBuilder builder,
        DomainRouteResolver routeResolver)
    {
        var route = PolicyRoute;
        var actions = new[] { "proposal.submit", "approve", "activate",
            "get", "versions", "active", "history",
            "governance.pending", "governance.approvals",
            "simulate", "simulate.batch",
            "federation.build", "federation.evaluate", "federation.get" };

        foreach (var action in actions)
        {
            var commandType = route.ResolveCommandType(action);
            builder.Workflows.Register(
                commandType,
                _ => Task.FromResult(new WorkflowStep[]
                {
                    new() { EngineCommandType = commandType }
                }));
        }
    }

    private static void RegisterIncidentWorkflow(
        RuntimeControlPlaneBuilder builder,
        DomainRouteResolver routeResolver)
    {
        var route = IncidentRoute;
        var actions = new[] { "create", "assign", "escalate", "resolve", "close" };

        foreach (var action in actions)
        {
            var commandType = route.ResolveCommandType(action);
            builder.Workflows.Register(
                commandType,
                _ => Task.FromResult(new WorkflowStep[]
                {
                    new() { EngineCommandType = commandType }
                }));
        }
    }
}
