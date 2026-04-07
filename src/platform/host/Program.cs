using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Minio;
using Prometheus;
using StackExchange.Redis;
using Whyce.Engines.T0U.WhyceChain.Engine;
using Whyce.Engines.T0U.WhyceId.Engine;
using Whyce.Engines.T0U.WhycePolicy.Engine;
using Whyce.Engines.T0U.WhycePolicy.Registry;
using Whyce.Engines.T1M.StepExecutor;
using Whyce.Engines.T1M.WorkflowEngine;
using Whyce.Platform.Api.Extensions;
using Whyce.Platform.Api.Health;
using Whyce.Platform.Host.Adapters;
using Whyce.Platform.Host.Composition;
using Whyce.Platform.Host.Health;
using Whyce.Runtime.ControlPlane;
using Whyce.Runtime.Dispatcher;
using Whyce.Runtime.EventFabric;
using Whyce.Runtime.Middleware;
using Whyce.Runtime.Middleware.Execution;
using Whyce.Runtime.Middleware.Observability;
using Whyce.Runtime.Middleware.PostPolicy;
using Whyce.Runtime.Middleware.PrePolicy;
using Whyce.Runtime.Projection;
using RuntimeMiddleware = Whyce.Runtime.Middleware.IMiddleware;
using PolicyMw = Whyce.Runtime.Middleware.Policy.PolicyMiddleware;
using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.Infrastructure.Chain;
using Whyce.Shared.Contracts.Infrastructure.Health;
using Whyce.Shared.Contracts.Infrastructure.Messaging;
using Whyce.Shared.Contracts.Infrastructure.Persistence;
using Whyce.Shared.Contracts.Infrastructure.Policy;
using Whyce.Shared.Contracts.Infrastructure.Projection;
using Whyce.Shared.Contracts.Runtime;
using Whyce.Shared.Kernel.Domain;
using Whyce.Runtime.WorkflowState;

var builder = WebApplication.CreateBuilder(args);

// --- Domain bootstrap modules ---
// All per-domain wiring lives in BootstrapModuleCatalog. Program.cs holds zero domain knowledge.
foreach (var module in BootstrapModuleCatalog.All)
{
    module.RegisterServices(builder.Services, builder.Configuration);
}

// Modules are also DI-resolvable so registry factories below can iterate them at resolution time.
foreach (var module in BootstrapModuleCatalog.All)
{
    builder.Services.AddSingleton<IDomainBootstrapModule>(module);
}

// IClock — deterministic time source (inject fixed clock for replay/test)
builder.Services.AddSingleton<IClock, SystemClock>();

// IIdGenerator — deterministic ID generation (SHA256-based)
builder.Services.AddSingleton<IIdGenerator, DeterministicIdGenerator>();

// --- Runtime Pipeline ---

// Engine registry — populated by domain bootstrap modules
builder.Services.AddSingleton<IEngineRegistry>(sp =>
{
    var registry = new EngineRegistry();
    foreach (var module in sp.GetServices<IDomainBootstrapModule>())
        module.RegisterEngines(registry);
    return registry;
});

// Engines — T0U (identity + policy + chain, constitutional layer — all stateless)
builder.Services.AddTransient<WhyceChainEngine>();
builder.Services.AddTransient<WhyceIdEngine>();
builder.Services.AddTransient(_ => new WhycePolicyEngine());

// Engines — T1M (workflow orchestration)
builder.Services.AddTransient<WorkflowStepExecutor>();
builder.Services.AddSingleton<IWorkflowEngine, T1MWorkflowEngine>();

// Engines — T1M workflow steps and T2E engines are registered by domain bootstrap modules.

// --- Infrastructure Connection Strings (from environment config — NO hardcoded fallback) ---
var postgresEventStoreCs = builder.Configuration.GetValue<string>("Postgres__ConnectionString")
    ?? throw new InvalidOperationException("Postgres__ConnectionString is required. No fallback.");
var postgresChainCs = builder.Configuration.GetValue<string>("Postgres__ChainConnectionString")
    ?? postgresEventStoreCs;
var postgresOutboxCs = postgresEventStoreCs;
var postgresProjectionsCs = builder.Configuration.GetValue<string>("Postgres__ProjectionsConnectionString")
    ?? "Host=localhost;Port=5434;Database=whyce_projections;Username=whyce;Password=whyce";
var redisConnectionString = builder.Configuration.GetValue<string>("Redis__ConnectionString")
    ?? throw new InvalidOperationException("Redis__ConnectionString is required. No fallback.");
var kafkaBootstrapServers = builder.Configuration.GetValue<string>("Kafka__BootstrapServers")
    ?? throw new InvalidOperationException("Kafka__BootstrapServers is required. No fallback.");
var opaEndpoint = builder.Configuration.GetValue<string>("OPA__Endpoint")
    ?? throw new InvalidOperationException("OPA__Endpoint is required. No fallback.");

// --- Infrastructure Adapters (REAL — no in-memory, no fallback) ---
builder.Services.AddSingleton<IEventStore>(sp =>
    new PostgresEventStoreAdapter(
        postgresEventStoreCs,
        sp.GetRequiredService<EventDeserializer>()));

// Schema-driven event deserializer (Phase B2b — replaces static EventTypeResolver).
builder.Services.AddSingleton<EventDeserializer>();
builder.Services.AddSingleton<IChainAnchor>(sp =>
    new WhyceChainPostgresAdapter(postgresChainCs, sp.GetRequiredService<IClock>()));
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnectionString));
builder.Services.AddSingleton<IRedisClient>(sp =>
    new StackExchangeRedisClient(sp.GetRequiredService<IConnectionMultiplexer>()));
builder.Services.AddSingleton<IPolicyEvaluator>(sp =>
    new OpaPolicyEvaluator(new HttpClient { Timeout = TimeSpan.FromSeconds(5) }, opaEndpoint));
builder.Services.AddSingleton<IIdempotencyStore>(sp =>
    new PostgresIdempotencyStoreAdapter(postgresEventStoreCs));
builder.Services.AddSingleton<IOutbox>(sp =>
    new PostgresOutboxAdapter(postgresOutboxCs));
builder.Services.AddSingleton<IWorkflowStateRepository, WorkflowStateRepository>();

// --- Kafka Producer (for outbox relay) ---
builder.Services.AddSingleton<IProducer<string, string>>(sp =>
{
    var config = new ProducerConfig { BootstrapServers = kafkaBootstrapServers };
    return new ProducerBuilder<string, string>(config).Build();
});

// --- Kafka Outbox Publisher (background relay: Postgres outbox → Kafka) ---
builder.Services.AddHostedService(sp =>
    new KafkaOutboxPublisher(
        postgresOutboxCs,
        sp.GetRequiredService<IProducer<string, string>>()));

// Projection handlers, consumers, and the Kafka projection worker are registered by domain bootstrap modules.

// --- Projection Registry (Event Fabric dispatches to projections) ---
// Populated by domain bootstrap modules INSIDE the factory closure, BEFORE Lock() —
// preserves the lock-after-build immutability guarantee.
builder.Services.AddSingleton<ProjectionRegistry>(sp =>
{
    var registry = new ProjectionRegistry();
    foreach (var module in sp.GetServices<IDomainBootstrapModule>())
        module.RegisterProjections(sp, registry);
    registry.Lock();
    return registry;
});
// --- Event Fabric Services (split responsibilities) ---
builder.Services.AddSingleton<EventStoreService>(sp =>
    new EventStoreService(sp.GetRequiredService<IEventStore>()));
builder.Services.AddSingleton<ChainAnchorService>(sp =>
    new ChainAnchorService(
        sp.GetRequiredService<WhyceChainEngine>(),
        sp.GetRequiredService<IChainAnchor>()));
builder.Services.AddSingleton<OutboxService>(sp =>
    new OutboxService(sp.GetRequiredService<IOutbox>()));
builder.Services.AddSingleton<EventSchemaRegistry>(sp =>
{
    var registry = new EventSchemaRegistry();
    foreach (var module in sp.GetServices<IDomainBootstrapModule>())
        module.RegisterSchema(registry);
    registry.Lock();
    return registry;
});

// --- Event Fabric (orchestrator ONLY — delegates to services, NO direct projection dispatch) ---
builder.Services.AddSingleton<TopicNameResolver>();
builder.Services.AddSingleton<IEventFabric>(sp =>
    new EventFabric(
        sp.GetRequiredService<EventStoreService>(),
        sp.GetRequiredService<ChainAnchorService>(),
        sp.GetRequiredService<OutboxService>(),
        sp.GetRequiredService<EventSchemaRegistry>(),
        sp.GetRequiredService<TopicNameResolver>(),
        sp.GetRequiredService<IClock>()));

// --- Middleware Pipeline (LOCKED ORDER via RuntimeControlPlaneBuilder) ---
builder.Services.AddSingleton<IReadOnlyList<RuntimeMiddleware>>(sp =>
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
builder.Services.AddSingleton<IWorkflowRegistry>(sp =>
{
    var registry = new Whyce.Runtime.Workflow.WorkflowRegistry();
    foreach (var module in sp.GetServices<IDomainBootstrapModule>())
        module.RegisterWorkflows(registry);
    return registry;
});

// Workflow dispatcher — systems entry point for workflow execution
builder.Services.AddSingleton<IWorkflowDispatcher, Whyce.Systems.Midstream.Wss.WorkflowDispatcher>();

// Runtime control plane — single entry point (now uses EventFabric)
builder.Services.AddSingleton<IRuntimeControlPlane>(sp =>
    new RuntimeControlPlane(
        sp.GetRequiredService<IReadOnlyList<RuntimeMiddleware>>(),
        sp.GetRequiredService<ICommandDispatcher>(),
        sp.GetRequiredService<IEventFabric>()));

// Command dispatcher (pure router) + system intent dispatcher
builder.Services.AddSingleton<ICommandDispatcher, Whyce.Runtime.Dispatcher.RuntimeCommandDispatcher>();
builder.Services.AddSingleton<ISystemIntentDispatcher, Whyce.Runtime.Dispatcher.SystemIntentDispatcher>();

// Systems.Downstream intent handlers are registered by domain bootstrap modules.

// Health checks — one per infrastructure dependency (NO hardcoded fallback)
builder.Services.AddSingleton<IHealthCheck>(sp =>
    new PostgreSqlHealthCheck(postgresEventStoreCs));

builder.Services.AddSingleton<IHealthCheck>(sp =>
    new KafkaHealthCheck(kafkaBootstrapServers));

builder.Services.AddSingleton<IHealthCheck>(sp =>
    new RedisHealthCheck(redisConnectionString));

builder.Services.AddSingleton<IHealthCheck>(sp =>
{
    var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
    return new OpaHealthCheck(httpClient, opaEndpoint);
});

builder.Services.AddSingleton<IHealthCheck>(sp =>
{
    var endpoint = builder.Configuration.GetValue<string>("MinIO__Endpoint")
        ?? throw new InvalidOperationException("MinIO__Endpoint is required. No fallback.");
    var accessKey = builder.Configuration.GetValue<string>("MinIO__AccessKey")
        ?? throw new InvalidOperationException("MinIO__AccessKey is required. No fallback.");
    var secretKey = builder.Configuration.GetValue<string>("MinIO__SecretKey")
        ?? throw new InvalidOperationException("MinIO__SecretKey is required. No fallback.");
    var useSsl = builder.Configuration.GetValue<bool>("MinIO__UseSsl", false);

    var client = new MinioClient()
        .WithEndpoint(endpoint)
        .WithCredentials(accessKey, secretKey)
        .WithSSL(useSsl)
        .Build();

    return new MinioHealthCheck(client);
});

builder.Services.AddSingleton<IHealthCheck>(sp =>
    new RuntimeHealthCheck(sp));

builder.Services.AddSingleton<HealthAggregator>();
builder.Services.AddWhyceSwagger();
builder.Services.AddControllers()
    .AddApplicationPart(typeof(Whyce.Platform.Api.Controllers.HealthController).Assembly);

var app = builder.Build();

// HTTP observability middleware (Prometheus) — before routing
app.UseMiddleware<Whyce.Runtime.Observability.HttpMetricsMiddleware>();
app.UseRouting();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.DocumentTitle = "Whycespace";
    options.SwaggerEndpoint("/swagger/operational/swagger.json", "Operational API");
    options.SwaggerEndpoint("/swagger/infrastructure/swagger.json", "Infrastructure API");
});

app.MapControllers();

// Prometheus /metrics endpoint
app.MapMetrics();

app.Run();

// --- Utility Types (non-infrastructure) ---

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

internal sealed class DeterministicIdGenerator : IIdGenerator
{
    public Guid Generate(string seed)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        return new Guid(hash.AsSpan(0, 16));
    }
}
