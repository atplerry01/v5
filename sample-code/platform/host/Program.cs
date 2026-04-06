using StackExchange.Redis;
using Whycespace.Platform.Api;
using Whycespace.Platform.Api.Core.Guards;
using Whycespace.Composition;
using Whycespace.Infrastructure.Adapters.Messaging.Kafka;
using Whycespace.Runtime.EventFabric.Outbox;
using Whycespace.Infrastructure.Adapters.Storage.Postgres;
using Whycespace.Runtime.Projection;
using Whycespace.Projections.Operational.Todo;
using Whycespace.Projections.Platform.Ping;
using Whycespace.Runtime.Bootstrap;
using Whycespace.Runtime.Workers;
using Whycespace.Infrastructure.Adapters.Redis;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.EventFabric.Consumers;
using Whycespace.Runtime.Routing;
using Whycespace.Shared.Contracts.Runtime;
using IProjectionStore = Whycespace.Shared.Contracts.Infrastructure.IProjectionStore;
using Whycespace.Shared.Contracts.Systems;
using Whycespace.Systems.Downstream.Operational.Incident;
using Whycespace.Systems.Downstream.Operational.Sandbox.Todo;
using Whycespace.Systems.Downstream.Platform.Ping;
using Whycespace.Systems.Downstream.Policy;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Infrastructure.Adapters.Chain;
using Whycespace.Shared.Contracts.Infrastructure.Storage;
using Whycespace.Shared.Primitives.Time;
using Whycespace.Systems.Midstream.Wss.Router;

// ── LAYER ISOLATION GUARD — fail-fast on architectural violations ──
LayerIsolationGuard.EnforceAtStartup();

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(
    builder.Configuration["ASPNETCORE_URLS"] ?? "http://+:5000");

// ── Performance: Thread Pool + Kestrel Tuning ──
// Scale .NET thread pool for high-concurrency event-sourced workloads.
// Defaults are too conservative for 1K+ RPS with async I/O to Postgres/Kafka/Redis.
{
    var minWorker = int.TryParse(builder.Configuration["ThreadPool:MinWorkers"], out var w) ? w : 200;
    var minIo = int.TryParse(builder.Configuration["ThreadPool:MinIO"], out var io) ? io : 200;
    ThreadPool.SetMinThreads(minWorker, minIo);
}

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxConcurrentConnections = 10_000;
    options.Limits.MaxConcurrentUpgradedConnections = 10_000;
    options.Limits.MaxRequestBodySize = 1_048_576; // 1MB — event payloads are small
    options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(120);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
});

// Determinism abstractions — injected across engines and runtime
builder.Services.AddSingleton<IClock>(SystemClock.Instance);
builder.Services.AddSingleton<IIdGenerator>(DefaultGuidGenerator.Instance);

// Infrastructure
builder.Services.AddSingleton<KafkaProducer>();
builder.Services.AddSingleton<Whycespace.Shared.Contracts.Infrastructure.IKafkaProducer>(sp =>
    sp.GetRequiredService<KafkaProducer>());

var postgresConnectionString = builder.Configuration["Postgres:ConnectionString"]
    ?? "Host=localhost;Port=5432;Database=whyce_eventstore;Username=whyce;Password=whyce";

// Append Npgsql connection pool tuning if not already specified
if (!postgresConnectionString.Contains("Pooling", StringComparison.OrdinalIgnoreCase))
{
    postgresConnectionString += ";Pooling=true;Minimum Pool Size=10;Maximum Pool Size=100;Connection Idle Lifetime=60;Connection Pruning Interval=10";
}
var redisConnectionString = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";

// Redis connection — shared across projection store and workers
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
builder.Services.AddSingleton<IProjectionStore>(sp =>
    new RedisProjectionStore(sp.GetRequiredService<IConnectionMultiplexer>()));

// Durable outbox store — PostgreSQL-backed, survives restarts.
// Events are persisted to Postgres BEFORE Kafka publish (outbox pattern guarantee).
// Uses SELECT ... FOR UPDATE SKIP LOCKED for concurrent worker safety.
builder.Services.AddSingleton<IOutboxStore>(sp =>
    new PostgresRuntimeOutboxStore(postgresConnectionString));

// Domain route resolver — shared across runtime, projections, and dispatch
builder.Services.AddSingleton<DomainRouteResolver>();

// P7: Persistent EventId idempotency store — Postgres-backed, survives restarts.
// Ensures every projection handler processes each event at most once.
builder.Services.AddSingleton<Whycespace.Runtime.EventFabric.Consumers.IEventIdempotencyStore>(sp =>
    new Whycespace.Infrastructure.Adapters.Storage.Postgres.PostgresEventIdempotencyStore(postgresConnectionString));

// Topic resolver — dual-topic model (domain + global for incident)
builder.Services.AddSingleton<TopicResolver>();

// Shared aggregate store — Redis-backed for multi-replica consistency.
// Aggregates cached with 30min TTL; on miss, fresh instance returned (event replay).
builder.Services.AddSingleton<IAggregateStore>(sp =>
    new Whycespace.Infrastructure.Adapters.Redis.RedisAggregateStore(
        sp.GetRequiredService<IConnectionMultiplexer>()));

// ── Sharded Event Store ──
// Distributes writes across N logical shards to eliminate cross-stream contention.
// Each shard has its own event_store_shard_{N} + outbox_shard_{N} tables.
var shardCount = int.TryParse(builder.Configuration["EventStore:ShardCount"], out var sc) ? sc : 4;
var shardConnectionStrings = new List<string>();
for (var i = 0; i < shardCount; i++)
{
    var shardConnStr = builder.Configuration[$"EventStore:Shards:{i}:ConnectionString"]
        ?? postgresConnectionString;
    shardConnectionStrings.Add(shardConnStr);
}

var shardConfig = new ShardConfiguration
{
    ShardCount = shardCount,
    ConnectionStrings = shardConnectionStrings
};

builder.Services.AddSingleton<IShardRouter>(new ShardRouter(shardConfig));
builder.Services.AddSingleton<ShardedEventStore>();

// Legacy single-table store (kept for backward compatibility reads)
var postgresEventStore = new PostgresRuntimeEventStore(postgresConnectionString);

// Runtime control plane — canonical pipeline with middleware + engine registration
builder.Services.AddSingleton<IEventPublisher>(sp =>
{
    var outboxStore = sp.GetRequiredService<IOutboxStore>();
    var router = new EventRouter();
    // Use sharded store for persistence
    var shardedStore = sp.GetRequiredService<ShardedEventStore>();
    router.UsePersistenceConsumer(new EventStoreConsumer(shardedStore));
    return new EventPublisher(outboxStore, router);
});
// ── WHYCEPOLICY + WhyceChain — delegated to Composition (layer isolation) ──
var opaUrl = builder.Configuration["Opa:Url"] ?? "http://localhost:8181";
builder.Services.AddSingleton<IChainStore>(new PostgresChainStore(postgresConnectionString));
CompositionRoot.RegisterPolicyAndChainEngines(builder.Services, opaUrl);

// Runtime infrastructure services — governance deps, domain services
RuntimeBootstrap.RegisterRuntimeServices(builder.Services);

// Runtime control plane — engine registration delegated to Composition (layer isolation)
builder.Services.AddSingleton<IRuntimeControlPlane>(CompositionRoot.BuildRuntimeControlPlane);

// ── Systems-First Flow: Platform → Downstream → WSS → Runtime ──

// WSS Router (Midstream) — resolves workflow + dispatches to runtime
builder.Services.AddSingleton<ContextResolver>();
builder.Services.AddSingleton<WorkflowResolver>();
builder.Services.AddSingleton<IWorkflowRouter>(sp =>
    new WorkflowRouter(
        sp.GetRequiredService<ContextResolver>(),
        sp.GetRequiredService<WorkflowResolver>(),
        sp.GetRequiredService<IRuntimeControlPlane>(),
        sp.GetRequiredService<IIdGenerator>()));

// Downstream Process Handlers — interpret business intent, route to WSS
builder.Services.AddSingleton<IProcessHandler>(sp =>
    new PingProcessHandler(sp.GetRequiredService<IWorkflowRouter>()));
builder.Services.AddSingleton<IProcessHandler>(sp =>
    new IncidentProcessHandler(sp.GetRequiredService<IWorkflowRouter>()));
builder.Services.AddSingleton<IProcessHandler>(sp =>
    new TodoProcessHandler(sp.GetRequiredService<IWorkflowRouter>()));
builder.Services.AddSingleton<IProcessHandler>(sp =>
    new PolicyProcessHandler(sp.GetRequiredService<IWorkflowRouter>()));

// Process Handler Registry — resolves handler by command type (used by DownstreamAdapter)
builder.Services.AddSingleton<IProcessHandlerRegistry>(sp =>
    new Whycespace.Systems.Downstream.ProcessHandlerRegistry(sp.GetServices<IProcessHandler>()));

// Projection handlers
builder.Services.AddSingleton<IProjectionHandler, TodoCreatedProjectionHandler>();
builder.Services.AddSingleton<IProjectionHandler, TodoCompletedProjectionHandler>();
builder.Services.AddSingleton<IProjectionHandler, PingProcessedProjectionHandler>();
builder.Services.AddSingleton<IProjectionHandler, Whycespace.Projections.Operational.Todo.TodoDomainCreatedProjectionHandler>();
builder.Services.AddSingleton<IProjectionHandler, Whycespace.Projections.Operational.Todo.TodoDomainCompletedProjectionHandler>();
builder.Services.AddSingleton<IProjectionHandler, Whycespace.Projections.Operational.Incident.IncidentCreatedProjectionHandler>();
builder.Services.AddSingleton<ProjectionRouter>();

// Workers
builder.Services.AddHostedService<SimpleEngineDispatcher>();
builder.Services.AddHostedService<CommandProcessorWorker>();
builder.Services.AddHostedService<KafkaConsumerWorker>();
builder.Services.AddHostedService<ProjectionWorker>();
builder.Services.AddHostedService<WorkflowWorker>();
builder.Services.AddHostedService<InMemoryOutboxPublisherWorker>();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Whycespace Foundation API",
        Version = "v1",
        Description = """
            <div style="margin-top:12px">
              <h4 style="margin:0 0 10px 0;color:#3b4151">Infrastructure Dashboards</h4>
              <div style="display:grid;grid-template-columns:repeat(3,1fr);gap:8px;max-width:720px">
                <div style="padding:10px 14px;background:#f7f7f7;border:1px solid #e0e0e0;border-radius:6px;font-size:13px">
                  <strong style="display:block;margin-bottom:4px;color:#3b4151">Kafka UI</strong>
                  <a href="http://localhost:8080" target="_blank" style="color:#4990e2;font-size:12px">localhost:8080</a>
                  <span style="color:#888;font-size:11px"> — Topics &amp; messages</span>
                </div>
                <div style="padding:10px 14px;background:#f7f7f7;border:1px solid #e0e0e0;border-radius:6px;font-size:13px">
                  <strong style="display:block;margin-bottom:4px;color:#3b4151">pgAdmin</strong>
                  <a href="http://localhost:5050" target="_blank" style="color:#4990e2;font-size:12px">localhost:5050</a>
                  <span style="color:#888;font-size:11px"> — Event store &amp; schemas</span>
                </div>
                <div style="padding:10px 14px;background:#f7f7f7;border:1px solid #e0e0e0;border-radius:6px;font-size:13px">
                  <strong style="display:block;margin-bottom:4px;color:#3b4151">RedisInsight</strong>
                  <a href="http://localhost:5540" target="_blank" style="color:#4990e2;font-size:12px">localhost:5540</a>
                  <span style="color:#888;font-size:11px"> — Cache &amp; workflow state</span>
                </div>
                <div style="padding:10px 14px;background:#f7f7f7;border:1px solid #e0e0e0;border-radius:6px;font-size:13px">
                  <strong style="display:block;margin-bottom:4px;color:#3b4151">Prometheus</strong>
                  <a href="http://localhost:9090" target="_blank" style="color:#4990e2;font-size:12px">localhost:9090</a>
                  <span style="color:#888;font-size:11px"> — Metrics &amp; targets</span>
                </div>
                <div style="padding:10px 14px;background:#f7f7f7;border:1px solid #e0e0e0;border-radius:6px;font-size:13px">
                  <strong style="display:block;margin-bottom:4px;color:#3b4151">Grafana</strong>
                  <a href="http://localhost:3000" target="_blank" style="color:#4990e2;font-size:12px">localhost:3000</a>
                  <span style="color:#888;font-size:11px"> — Dashboards &amp; alerts</span>
                </div>
                <div style="padding:10px 14px;background:#f7f7f7;border:1px solid #e0e0e0;border-radius:6px;font-size:13px">
                  <strong style="display:block;margin-bottom:4px;color:#3b4151">OPA</strong>
                  <a href="http://localhost:8181" target="_blank" style="color:#4990e2;font-size:12px">localhost:8181</a>
                  <span style="color:#888;font-size:11px"> — Policy rules &amp; evaluation</span>
                </div>
              </div>
            </div>
            """
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Whycespace Foundation API v1");
    c.RoutePrefix = "swagger";
});

// ── Domain-scoped API endpoints ──
app.MapApiEndpoints();

app.Run();
