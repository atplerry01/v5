using Npgsql;
using StackExchange.Redis;
using Whycespace.Engines.T0U.Determinism;
using Whycespace.Engines.T0U.Determinism.Sequence;
using Whycespace.Engines.T0U.Determinism.Time;
using Whycespace.Engines.T0U.WhyceChain.Engine;
using Whycespace.Engines.T0U.WhyceId.Engine;
using Whycespace.Engines.T0U.WhycePolicy.Engine;
using Whycespace.Engines.T2E.Operational.Sandbox.Todo;
using Whycespace.Platform.Host.Adapters;
using Whycespace.Platform.Host.Runtime;
using Whycespace.Runtime.ControlPlane;
using Whycespace.Runtime.Dispatcher;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Middleware;
using Whycespace.Runtime.Middleware.Execution;
using Whycespace.Runtime.Middleware.Observability;
using Whycespace.Runtime.Middleware.PostPolicy;
using Whycespace.Runtime.Middleware.PrePolicy;
using Whycespace.Runtime.Topology;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Infrastructure.Admission;
using Whycespace.Shared.Contracts.Infrastructure.Health;
using Whycespace.Shared.Contracts.Infrastructure.Messaging;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Contracts.Operational.Sandbox.Todo;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Determinism;
using Whycespace.Shared.Kernel.Domain;
using Whycespace.Tests.Integration.Setup;
using Whycespace.Tests.Shared;
using PolicyMw = Whycespace.Runtime.Middleware.Policy.PolicyMiddleware;

namespace Whycespace.Tests.Integration.EconomicSystem.Shared;

/// <summary>
/// phase5-operational-activation + phase6-hardening: real-infrastructure
/// equivalent of <see cref="TestHost"/> for the economic-system
/// certification suite.
///
/// Activated when the <c>REAL_INFRA=true</c> environment flag is set (see
/// <see cref="TestHost.ForTodo"/>). Wires the exact same runtime pipeline
/// that <see cref="TestHost.ForTodo"/> builds in-memory, but swaps the
/// durability adapters for their production counterparts:
///
///   event store       → <see cref="PostgresEventStoreAdapter"/>
///   outbox            → <see cref="PostgresOutboxAdapter"/>
///   idempotency store → <see cref="PostgresIdempotencyStoreAdapter"/>
///   chain anchor      → <see cref="WhyceChainPostgresAdapter"/>
///   execution lock    → <see cref="RedisExecutionLockProvider"/>
///
/// Each real adapter is wrapped in a mirrored observer (Mirrored{EventStore,
/// Outbox, ChainAnchor}) so the existing certification test bodies that
/// read <c>host.EventStore.AllEvents(id)</c> / <c>host.Outbox.Batches</c> /
/// <c>host.ChainAnchor.Blocks</c> continue to compile and to observe the
/// events that just flowed through real Postgres / Redis. The mirror is
/// populated ONLY AFTER the real adapter succeeds — there is no silent
/// fallback to in-memory state.
///
/// Connection strings and broker endpoints are sourced from environment
/// variables (no hardcoded defaults per R-CFG-R2 / R-CFG-R3):
///
///   Postgres__ConnectionString        — event store + outbox + idempotency
///   Postgres__ChainConnectionString   — whyce_chain table
///   Redis__ConnectionString           — execution lock
///   Kafka__BootstrapServers           — outbox → broker relay (optional)
///
/// The populated values come from <c>infrastructure/docker/.env.local</c>
/// when invoked through <c>scripts/validate.sh --real-infra</c>.
/// </summary>
internal static class RealInfraTestHost
{
    public static TestHost ForTodo(
        IClock? clock = null,
        IIdGenerator? idGenerator = null,
        bool denyPolicy = false,
        Whycespace.Shared.Contracts.Infrastructure.Policy.IPolicyEvaluator? policyEvaluator = null,
        Whycespace.Shared.Contracts.Infrastructure.Chain.IChainAnchor? chainAnchorOverride = null)
    {
        clock ??= new TestClock();
        idGenerator ??= new TestIdGenerator();
        var recorder = new StageRecorder();

        // --- Real Postgres / Redis wiring ---------------------------------

        var eventStoreConn = RequireEnv("Postgres__ConnectionString");
        var chainConn = Environment.GetEnvironmentVariable("Postgres__ChainConnectionString")
            ?? eventStoreConn;
        var redisConn = RequireEnv("Redis__ConnectionString");

        var eventStoreDataSource = new EventStoreDataSource(NpgsqlDataSource.Create(eventStoreConn));
        var chainDataSource = new ChainDataSource(NpgsqlDataSource.Create(chainConn));
        var redis = ConnectionMultiplexer.Connect(redisConn);

        var schemaRegistry = new EventSchemaRegistry();
        schemaRegistry.Lock();
        var topicResolver = new TopicNameResolver();
        var deserializer = new EventDeserializer(schemaRegistry);

        IEventStore realEventStore = new PostgresEventStoreAdapter(
            eventStoreDataSource, deserializer, idGenerator);

        var outboxOptions = new OutboxOptions();
        var depthSnapshot = new RealInfraOutboxDepthSnapshot();
        IOutbox realOutbox = new PostgresOutboxAdapter(
            eventStoreDataSource, depthSnapshot, outboxOptions, clock, schemaRegistry);

        IIdempotencyStore realIdempotency = new PostgresIdempotencyStoreAdapter(eventStoreDataSource);

        var whyceIdEngine = new WhyceIdEngine();
        var whycePolicyEngine = new WhycePolicyEngine();
        var whyceChainEngine = new WhyceChainEngine();
        var chainOptions = new ChainAnchorOptions();
        Whycespace.Shared.Contracts.Infrastructure.Chain.IChainAnchor realChainAnchor =
            new WhyceChainPostgresAdapter(chainDataSource, clock, chainOptions);

        var resolvedPolicyEvaluator = policyEvaluator
            ?? new AllowAllPolicyEvaluator { ShouldDeny = denyPolicy };

        var lockProvider = new RedisExecutionLockProvider(redis);

        // --- Mirrors so tests can still inspect after real persistence ----

        var mirrorEventStore = new InMemoryEventStore(recorder);
        var mirrorOutbox = new InMemoryOutbox(recorder);
        var mirrorChainAnchor = new InMemoryChainAnchor(clock, idGenerator, recorder);

        IEventStore mirroredEventStore = new MirroredEventStore(realEventStore, mirrorEventStore);
        IOutbox mirroredOutbox = new MirroredOutbox(realOutbox, mirrorOutbox);
        var mirroredChainAnchor = new MirroredChainAnchor(
            chainAnchorOverride ?? realChainAnchor, mirrorChainAnchor);

        // --- Runtime pipeline (identical shape to TestHost.ForTodo) -------

        var eventStoreService = new EventStoreService(mirroredEventStore);
        var chainAnchorService = new ChainAnchorService(
            whyceChainEngine, mirroredChainAnchor, chainOptions);
        var outboxService = new OutboxService(mirroredOutbox);
        var eventFabric = new EventFabric(
            eventStoreService, chainAnchorService, outboxService,
            schemaRegistry, topicResolver, clock);

        var engineRegistry = new EngineRegistry();
        engineRegistry.Register<CreateTodoCommand, CreateTodoHandler>();
        engineRegistry.Register<UpdateTodoCommand, UpdateTodoHandler>();
        engineRegistry.Register<CompleteTodoCommand, CompleteTodoHandler>();

        var services = new RealInfraServiceProvider();
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
            mirroredEventStore,
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
                resolvedPolicyEvaluator,
                idGenerator,
                new Whycespace.Engines.T0U.WhycePolicy.PolicyDecisionEventFactory(),
                new TestNoOpCallerIdentityAccessor(),
                clock,
                new Whycespace.Runtime.Middleware.Policy.NullAggregateStateLoader()))
            .UseAuthorizationGuard(new AuthorizationGuardMiddleware())
            .UseIdempotency(new IdempotencyMiddleware(realIdempotency))
            .UseExecutionGuard(new ExecutionGuardMiddleware());

        var realMiddlewares = builder.Build();
        var recordedMiddlewares = new IMiddleware[realMiddlewares.Count];
        for (var i = 0; i < realMiddlewares.Count; i++)
        {
            var name = realMiddlewares[i].GetType().Name.Replace("Middleware", string.Empty);
            recordedMiddlewares[i] = new RecordingMiddleware(realMiddlewares[i], name, recorder);
        }

        var hsidEngine = new DeterministicIdEngine(new DeterministicTimeBucketProvider());
        var sequenceStore = new InMemorySequenceStore();
        var sequenceResolver = new PersistedSequenceResolver(sequenceStore);
        var topologyResolver = new TopologyResolver(
            new InMemoryStructureRegistry(Array.Empty<StructureNode>()));

        var stateAggregator = new RealInfraStateAggregator();
        var maintenanceProvider = new RealInfraMaintenanceModeProvider();

        var controlPlane = new RuntimeControlPlane(
            recordedMiddlewares,
            dispatcher,
            eventFabric,
            hsidEngine,
            sequenceResolver,
            topologyResolver,
            stateAggregator,
            maintenanceProvider,
            lockProvider);

        return new TestHost(
            controlPlane, mirrorEventStore, mirrorChainAnchor, mirrorOutbox,
            recorder, clock, idGenerator);
    }

    private static string RequireEnv(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"REAL_INFRA mode requires environment variable '{key}' to be set. " +
                "Source infrastructure/docker/.env.local or invoke via scripts/validate.sh --real-infra.");
        }
        return value;
    }
}

internal sealed class RealInfraServiceProvider : IServiceProvider
{
    private readonly Dictionary<Type, object> _services = new();
    public void Register(Type type, object instance) => _services[type] = instance;
    public object? GetService(Type serviceType) => _services.GetValueOrDefault(serviceType);
}

internal sealed class RealInfraStateAggregator : Whycespace.Shared.Contracts.Infrastructure.Health.IRuntimeStateAggregator
{
    public Task<Whycespace.Shared.Contracts.Infrastructure.Health.RuntimeStateSnapshot> GetCurrentStateAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(new Whycespace.Shared.Contracts.Infrastructure.Health.RuntimeStateSnapshot(
            Whycespace.Shared.Contracts.Infrastructure.Health.RuntimeState.Healthy, Array.Empty<string>()));

    public Whycespace.Shared.Contracts.Infrastructure.Health.RuntimeStateSnapshot ComputeFromResults(
        IReadOnlyList<Whycespace.Shared.Contracts.Infrastructure.Health.HealthCheckResult> results)
        => new(Whycespace.Shared.Contracts.Infrastructure.Health.RuntimeState.Healthy, Array.Empty<string>());

    public Whycespace.Shared.Contracts.Infrastructure.Health.RuntimeDegradedMode GetDegradedMode() =>
        Whycespace.Shared.Contracts.Infrastructure.Health.RuntimeDegradedMode.None;
}

internal sealed class RealInfraMaintenanceModeProvider : IRuntimeMaintenanceModeProvider
{
    public RuntimeMaintenanceMode Get() => RuntimeMaintenanceMode.None;
}
