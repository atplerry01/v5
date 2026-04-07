using Whyce.Engines.T0U.Determinism;
using Whyce.Engines.T0U.Determinism.Sequence;
using Whyce.Engines.T0U.Determinism.Time;
using Whyce.Engines.T0U.WhyceChain.Engine;
using Whyce.Engines.T0U.WhyceId.Engine;
using Whyce.Engines.T0U.WhycePolicy.Engine;
using Whyce.Runtime.Topology;
using Whyce.Shared.Kernel.Determinism;
using Whyce.Engines.T2E.Operational.Todo;
using Whyce.Runtime.ControlPlane;
using Whyce.Runtime.Dispatcher;
using Whyce.Runtime.EventFabric;
using Whyce.Runtime.Middleware;
using Whyce.Runtime.Middleware.Execution;
using Whyce.Runtime.Middleware.Observability;
using Whyce.Runtime.Middleware.PostPolicy;
using Whyce.Runtime.Middleware.PrePolicy;
using Whyce.Shared.Contracts.Application.Todo;
using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.Runtime;
using Whyce.Shared.Kernel.Domain;
using Whycespace.Tests.Shared;
using PolicyMw = Whyce.Runtime.Middleware.Policy.PolicyMiddleware;

namespace Whycespace.Tests.Integration.Setup;

/// <summary>
/// Builds a fully-wired RuntimeControlPlane for integration tests.
///
/// Real components: middleware pipeline (all 8), RuntimeCommandDispatcher,
/// EventFabric, EventStoreService, ChainAnchorService, OutboxService,
/// EventSchemaRegistry, TopicNameResolver, WhyceIdEngine, WhycePolicyEngine,
/// WhyceChainEngine, EngineRegistry, TodoEngine.
///
/// In-memory substitutes for infrastructure adapters: IEventStore,
/// IChainAnchor, IOutbox, IIdempotencyStore, IPolicyEvaluator (OPA).
///
/// Workflow surface is stubbed (Todo doesn't use workflows).
/// IClock and IIdGenerator are deterministic test seams.
/// </summary>
public sealed class TestHost
{
    public IRuntimeControlPlane ControlPlane { get; }
    public InMemoryEventStore EventStore { get; }
    public InMemoryChainAnchor ChainAnchor { get; }
    public InMemoryOutbox Outbox { get; }
    public StageRecorder Recorder { get; }
    public IClock Clock { get; }
    public IIdGenerator IdGenerator { get; }

    public TestHost(
        IRuntimeControlPlane controlPlane,
        InMemoryEventStore eventStore,
        InMemoryChainAnchor chainAnchor,
        InMemoryOutbox outbox,
        StageRecorder recorder,
        IClock clock,
        IIdGenerator idGenerator)
    {
        ControlPlane = controlPlane;
        EventStore = eventStore;
        ChainAnchor = chainAnchor;
        Outbox = outbox;
        Recorder = recorder;
        Clock = clock;
        IdGenerator = idGenerator;
    }

    /// <summary>
    /// Build a TestHost for the Todo domain. Uses TestClock + TestIdGenerator
    /// by default; supply custom seams (e.g. a different frozen clock) for
    /// special-purpose tests.
    /// </summary>
    public static TestHost ForTodo(
        IClock? clock = null,
        IIdGenerator? idGenerator = null,
        bool denyPolicy = false)
    {
        clock ??= new TestClock();
        idGenerator ??= new TestIdGenerator();
        var recorder = new StageRecorder();

        // T0U engines (parameterless)
        var whyceIdEngine = new WhyceIdEngine();
        var whycePolicyEngine = new WhycePolicyEngine();
        var whyceChainEngine = new WhyceChainEngine();
        var policyEvaluator = new AllowAllPolicyEvaluator { ShouldDeny = denyPolicy };

        // In-memory adapters
        var eventStore = new InMemoryEventStore(recorder);
        var chainAnchor = new InMemoryChainAnchor(clock, idGenerator, recorder);
        var outbox = new InMemoryOutbox(recorder);
        var idempotencyStore = new InMemoryIdempotencyStore();

        // Event fabric services
        var eventStoreService = new EventStoreService(eventStore);
        var chainAnchorService = new ChainAnchorService(whyceChainEngine, chainAnchor);
        var outboxService = new OutboxService(outbox);
        var schemaRegistry = new EventSchemaRegistry();
        // Todo events are auto-registered via the default fallback in EventSchemaRegistry.Resolve,
        // so explicit registration is not strictly required for the integration tests.
        // Lock to enforce immutability post-build.
        schemaRegistry.Lock();
        var topicResolver = new TopicNameResolver();
        var eventFabric = new EventFabric(
            eventStoreService, chainAnchorService, outboxService,
            schemaRegistry, topicResolver, clock);

        // Engine registry + service provider (Todo engines)
        var engineRegistry = new EngineRegistry();
        engineRegistry.Register<CreateTodoCommand, TodoEngine>();
        engineRegistry.Register<UpdateTodoCommand, TodoEngine>();
        engineRegistry.Register<CompleteTodoCommand, TodoEngine>();

        var services = new MinimalServiceProvider();
        services.Register(typeof(TodoEngine), new TodoEngine());

        // Workflow surface (stubbed — Todo doesn't use it)
        var workflowEngine = new NoOpWorkflowEngine();
        var workflowRegistry = new NoOpWorkflowRegistry();
        var replayService = new NoOpWorkflowExecutionReplayService();

        // Real RuntimeCommandDispatcher
        var dispatcher = new RuntimeCommandDispatcher(
            engineRegistry,
            services,
            eventStore,
            workflowEngine,
            workflowRegistry,
            replayService);

        // Build the locked middleware pipeline via RuntimeControlPlaneBuilder,
        // then wrap each middleware in a RecordingMiddleware so the test can
        // witness the order at runtime. Wrapping post-Build preserves the real
        // production order — we are observing it, not redefining it.
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
                new Whyce.Engines.T0U.WhycePolicy.PolicyDecisionEventFactory()))
            .UseAuthorizationGuard(new AuthorizationGuardMiddleware())
            .UseIdempotency(new IdempotencyMiddleware(idempotencyStore))
            .UseExecutionGuard(new ExecutionGuardMiddleware());

        var realMiddlewares = builder.Build();
        var recordedMiddlewares = new IMiddleware[realMiddlewares.Count];
        for (var i = 0; i < realMiddlewares.Count; i++)
        {
            var name = realMiddlewares[i].GetType().Name.Replace("Middleware", string.Empty);
            recordedMiddlewares[i] = new RecordingMiddleware(realMiddlewares[i], name, recorder);
        }

        // HSID v2.1 hardening — mandatory deterministic identity stack.
        var hsidEngine = new DeterministicIdEngine(new DeterministicTimeBucketProvider());
        var sequenceStore = new InMemorySequenceStore();
        var sequenceResolver = new PersistedSequenceResolver(sequenceStore);
        var topologyResolver = new TopologyResolver(
            new InMemoryStructureRegistry(Array.Empty<StructureNode>()));

        var controlPlane = new RuntimeControlPlane(
            recordedMiddlewares,
            dispatcher,
            eventFabric,
            hsidEngine,
            sequenceResolver,
            topologyResolver);

        return new TestHost(controlPlane, eventStore, chainAnchor, outbox, recorder, clock, idGenerator);
    }

    /// <summary>
    /// Builds a fresh CommandContext for the Todo domain. Each Dispatch needs a
    /// new context because IdentityId / PolicyDecision* are write-once on
    /// CommandContext (see src/shared/contracts/runtime/CommandContext.cs).
    /// </summary>
    public CommandContext NewTodoContext(Guid aggregateId, Guid? correlationId = null) => new()
    {
        CorrelationId = correlationId ?? IdGenerator.Generate($"corr:{aggregateId}"),
        CausationId = IdGenerator.Generate($"cause:{aggregateId}"),
        CommandId = IdGenerator.Generate($"cmd:{aggregateId}"),
        TenantId = "test-tenant",
        ActorId = "test-actor",
        AggregateId = aggregateId,
        PolicyId = "default",
        Classification = "operational",
        Context = "sandbox",
        Domain = "todo"
    };
}

internal sealed class MinimalServiceProvider : IServiceProvider
{
    private readonly Dictionary<Type, object> _services = new();
    public void Register(Type type, object instance) => _services[type] = instance;
    public object? GetService(Type serviceType) => _services.GetValueOrDefault(serviceType);
}
