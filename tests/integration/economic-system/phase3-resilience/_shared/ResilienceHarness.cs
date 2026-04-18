using Whycespace.Engines.T0U.Determinism;
using Whycespace.Engines.T0U.Determinism.Sequence;
using Whycespace.Engines.T0U.Determinism.Time;
using Whycespace.Engines.T0U.WhyceChain.Engine;
using Whycespace.Engines.T0U.WhyceId.Engine;
using Whycespace.Engines.T0U.WhycePolicy.Engine;
using Whycespace.Runtime.Topology;
using Whycespace.Shared.Kernel.Determinism;
using Whycespace.Engines.T2E.Operational.Sandbox.Todo;
using Whycespace.Runtime.ControlPlane;
using Whycespace.Runtime.Dispatcher;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Middleware;
using Whycespace.Runtime.Middleware.Execution;
using Whycespace.Runtime.Middleware.Observability;
using Whycespace.Runtime.Middleware.PostPolicy;
using Whycespace.Runtime.Middleware.PrePolicy;
using Whycespace.Shared.Contracts.Operational.Sandbox.Todo;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Infrastructure.Admission;
using Whycespace.Shared.Contracts.Infrastructure.Health;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;
using Whycespace.Tests.Integration.Setup;
using Whycespace.Tests.Shared;
using PolicyMw = Whycespace.Runtime.Middleware.Policy.PolicyMiddleware;

namespace Whycespace.Tests.Integration.EconomicSystem.Phase3Resilience.Shared;

/// <summary>
/// Phase 3 resilience harness. Mirrors the Phase 2 <see cref="TestHost"/>
/// wiring but exposes seams required by the Phase 3 failure suite — in
/// particular, the ability to swap in a <see cref="FailingEventStore"/>
/// wrapper over the real <see cref="InMemoryEventStore"/>.
///
/// Not a fork of TestHost — the Phase 3 scope in
/// <c>pipeline/execution_context_phase3_resilience.md</c> limits
/// modifications to the Phase 3 subtree; TestHost remains byte-identical
/// for Phase 2. When the seam shapes stabilise, TestHost may be updated
/// in a follow-up batch to accept these overrides directly.
/// </summary>
public sealed class ResilienceHarness
{
    public IRuntimeControlPlane ControlPlane { get; }
    public InMemoryEventStore EventStore { get; }
    public FailingEventStore? FailingStore { get; }
    public InMemoryChainAnchor ChainAnchor { get; }
    public InMemoryOutbox Outbox { get; }
    public StageRecorder Recorder { get; }
    public IClock Clock { get; }
    public IIdGenerator IdGenerator { get; }
    public MetricsCollector Metrics { get; }
    public Tracer Tracer { get; }

    private ResilienceHarness(
        IRuntimeControlPlane controlPlane,
        InMemoryEventStore eventStore,
        FailingEventStore? failingStore,
        InMemoryChainAnchor chainAnchor,
        InMemoryOutbox outbox,
        StageRecorder recorder,
        IClock clock,
        IIdGenerator idGenerator,
        MetricsCollector metrics,
        Tracer tracer)
    {
        ControlPlane = controlPlane;
        EventStore = eventStore;
        FailingStore = failingStore;
        ChainAnchor = chainAnchor;
        Outbox = outbox;
        Recorder = recorder;
        Clock = clock;
        IdGenerator = idGenerator;
        Metrics = metrics;
        Tracer = tracer;
    }

    public static ResilienceHarness Build(int failuresToInject = 0)
    {
        var clock = new TestClock();
        var idGenerator = new TestIdGenerator();
        var recorder = new StageRecorder();
        var metrics = new MetricsCollector();
        var tracer = new Tracer();

        var whyceIdEngine = new WhyceIdEngine();
        var whycePolicyEngine = new WhycePolicyEngine();
        var whyceChainEngine = new WhyceChainEngine();
        var policyEvaluator = new AllowAllPolicyEvaluator();

        var innerStore = new InMemoryEventStore(recorder);
        FailingEventStore? failingStore = failuresToInject > 0
            ? new FailingEventStore(innerStore, failuresToInject)
            : null;
        IEventStore resolvedStore = (IEventStore?)failingStore ?? innerStore;

        var chainAnchor = new InMemoryChainAnchor(clock, idGenerator, recorder);
        var outbox = new InMemoryOutbox(recorder);
        var idempotencyStore = new InMemoryIdempotencyStore();

        var eventStoreService = new EventStoreService(resolvedStore);
        var chainAnchorService = new ChainAnchorService(
            whyceChainEngine, chainAnchor, new ChainAnchorOptions());
        var outboxService = new OutboxService(outbox);
        var schemaRegistry = new EventSchemaRegistry();
        schemaRegistry.Lock();
        var topicResolver = new TopicNameResolver();
        var eventFabric = new EventFabric(
            eventStoreService, chainAnchorService, outboxService,
            schemaRegistry, topicResolver, clock);

        var engineRegistry = new EngineRegistry();
        engineRegistry.Register<CreateTodoCommand, CreateTodoHandler>();
        engineRegistry.Register<UpdateTodoCommand, UpdateTodoHandler>();
        engineRegistry.Register<CompleteTodoCommand, CompleteTodoHandler>();

        var services = new HarnessServiceProvider();
        services.Register(typeof(CreateTodoHandler), new CreateTodoHandler());
        services.Register(typeof(UpdateTodoHandler), new UpdateTodoHandler());
        services.Register(typeof(CompleteTodoHandler), new CompleteTodoHandler());

        var workflowEngine = new NoOpWorkflowEngine();
        var workflowRegistry = new NoOpWorkflowRegistry();
        var replayService = new NoOpWorkflowExecutionReplayService();
        var workflowAdmissionGate = new WorkflowAdmissionGate(new WorkflowOptions());

        var dispatcher = new RuntimeCommandDispatcher(
            engineRegistry,
            services,
            resolvedStore,
            workflowEngine,
            workflowRegistry,
            replayService,
            workflowAdmissionGate);

        var builder = new RuntimeControlPlaneBuilder()
            .UseTracing(new TracingMiddleware())
            .UseMetrics(new MetricsMiddleware())
            .UseContextGuard(new ContextGuardMiddleware())
            .UseValidation(new ValidationMiddleware())
            .UsePolicy(new PolicyMw(
                whyceIdEngine,
                whycePolicyEngine,
                policyEvaluator,
                idGenerator,
                new Whycespace.Engines.T0U.WhycePolicy.PolicyDecisionEventFactory(),
                new TestNoOpCallerIdentityAccessor(),
                clock,
                new Whycespace.Runtime.Middleware.Policy.NullAggregateStateLoader()))
            .UseAuthorizationGuard(new AuthorizationGuardMiddleware())
            .UseIdempotency(new IdempotencyMiddleware(idempotencyStore))
            .UseExecutionGuard(new ExecutionGuardMiddleware());

        var middlewares = builder.Build();

        var hsidEngine = new DeterministicIdEngine(new DeterministicTimeBucketProvider());
        var sequenceStore = new InMemorySequenceStore();
        var sequenceResolver = new PersistedSequenceResolver(sequenceStore);
        var topologyResolver = new TopologyResolver(
            new InMemoryStructureRegistry(Array.Empty<StructureNode>()));

        var stateAggregator = new HarnessRuntimeStateAggregator();
        var maintenanceProvider = new HarnessMaintenanceModeProvider();
        var lockProvider = new HarnessExecutionLockProvider();

        var controlPlane = new RuntimeControlPlane(
            middlewares,
            dispatcher,
            eventFabric,
            hsidEngine,
            sequenceResolver,
            topologyResolver,
            stateAggregator,
            maintenanceProvider,
            lockProvider);

        return new ResilienceHarness(
            controlPlane,
            innerStore,
            failingStore,
            chainAnchor,
            outbox,
            recorder,
            clock,
            idGenerator,
            metrics,
            tracer);
    }

    public CommandContext NewTodoContext(Guid aggregateId, Guid? correlationId = null, Guid? commandId = null) => new()
    {
        CorrelationId = correlationId ?? IdGenerator.Generate($"corr:{aggregateId}"),
        CausationId = IdGenerator.Generate($"cause:{aggregateId}"),
        CommandId = commandId ?? IdGenerator.Generate($"cmd:{aggregateId}"),
        TenantId = "phase3-tenant",
        ActorId = "phase3-actor",
        AggregateId = aggregateId,
        PolicyId = "default",
        Classification = "operational",
        Context = "sandbox",
        Domain = "todo"
    };
}

internal sealed class HarnessServiceProvider : IServiceProvider
{
    private readonly Dictionary<Type, object> _services = new();
    public void Register(Type type, object instance) => _services[type] = instance;
    public object? GetService(Type serviceType) => _services.GetValueOrDefault(serviceType);
}

internal sealed class HarnessRuntimeStateAggregator : IRuntimeStateAggregator
{
    public Task<RuntimeStateSnapshot> GetCurrentStateAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(new RuntimeStateSnapshot(RuntimeState.Healthy, Array.Empty<string>()));

    public RuntimeStateSnapshot ComputeFromResults(IReadOnlyList<HealthCheckResult> results)
        => new(RuntimeState.Healthy, Array.Empty<string>());

    public RuntimeDegradedMode GetDegradedMode() => RuntimeDegradedMode.None;
}

internal sealed class HarnessMaintenanceModeProvider : IRuntimeMaintenanceModeProvider
{
    public RuntimeMaintenanceMode Get() => RuntimeMaintenanceMode.None;
}

internal sealed class HarnessExecutionLockProvider : IExecutionLockProvider
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, byte> _held =
        new(StringComparer.Ordinal);

    public Task<bool> TryAcquireAsync(string key, TimeSpan ttl, CancellationToken ct)
        => Task.FromResult(_held.TryAdd(key, 1));

    public Task ReleaseAsync(string key)
    {
        _held.TryRemove(key, out _);
        return Task.CompletedTask;
    }
}
