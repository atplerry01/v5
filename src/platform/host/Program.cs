using Minio;
using Prometheus;
using Whyce.Engines.T2E.Operational.Todo;
using Whyce.Platform.Host.Health;
using Whyce.Runtime.Observability;
using Whyce.Runtime.Pipeline;
using RuntimeMiddleware = Whyce.Runtime.Pipeline.IMiddleware;
using Whyce.Shared.Contracts.Application.Todo;
using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.Infrastructure.Chain;
using Whyce.Shared.Contracts.Infrastructure.Health;
using Whyce.Shared.Contracts.Infrastructure.Messaging;
using Whyce.Shared.Contracts.Infrastructure.Persistence;
using Whyce.Shared.Contracts.Infrastructure.Policy;
using Whyce.Shared.Contracts.Runtime;
using Whyce.Shared.Kernel.Domain;

var builder = WebApplication.CreateBuilder(args);

// IClock — deterministic time source
builder.Services.AddSingleton<IClock, SystemClock>();

// --- Runtime Pipeline ---

// Engine registry — bind commands to engines
builder.Services.AddSingleton<IEngineRegistry>(sp =>
{
    var registry = new EngineRegistry();
    registry.Register<CreateTodoCommand, TodoEngine>();
    registry.Register<UpdateTodoCommand, TodoEngine>();
    registry.Register<CompleteTodoCommand, TodoEngine>();
    return registry;
});

// Engines
builder.Services.AddTransient<TodoEngine>();

// Infrastructure adapters (in-memory for Phase 1 sandbox)
builder.Services.AddSingleton<IEventStore, InMemoryEventStore>();
builder.Services.AddSingleton<IChainAnchor, InMemoryChainAnchor>();
builder.Services.AddSingleton<IOutbox, InMemoryOutbox>();
builder.Services.AddSingleton<IPolicyEvaluator, AllowAllPolicyEvaluator>();
builder.Services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();

// Middleware pipeline (order matters: context guard → idempotency → policy)
builder.Services.AddSingleton<RuntimeMiddleware, ContextGuardMiddleware>();
builder.Services.AddSingleton<RuntimeMiddleware>(sp =>
    new IdempotencyMiddleware(sp.GetRequiredService<IIdempotencyStore>()));
builder.Services.AddSingleton<RuntimeMiddleware>(sp =>
    new PolicyMiddleware(sp.GetRequiredService<IPolicyEvaluator>()));

// Command dispatcher + system intent dispatcher
builder.Services.AddSingleton<ICommandDispatcher, RuntimeCommandDispatcher>();
builder.Services.AddSingleton<ISystemIntentDispatcher, SystemIntentDispatcher>();

// Health checks — one per infrastructure dependency
builder.Services.AddSingleton<IHealthCheck>(sp =>
    new PostgreSqlHealthCheck(
        builder.Configuration.GetValue<string>("Postgres__ConnectionString")
            ?? "Host=localhost;Port=5432;Database=whyce_eventstore;Username=whyce;Password=whyce"));

builder.Services.AddSingleton<IHealthCheck>(sp =>
    new KafkaHealthCheck(
        builder.Configuration.GetValue<string>("Kafka__BootstrapServers")
            ?? "localhost:29092"));

builder.Services.AddSingleton<IHealthCheck>(sp =>
    new RedisHealthCheck(
        builder.Configuration.GetValue<string>("Redis__ConnectionString")
            ?? "localhost:6379,abortConnect=false"));

builder.Services.AddSingleton<IHealthCheck>(sp =>
{
    var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
    var opaEndpoint = builder.Configuration.GetValue<string>("OPA__Endpoint")
        ?? "http://localhost:8181";
    return new OpaHealthCheck(httpClient, opaEndpoint);
});

builder.Services.AddSingleton<IHealthCheck>(sp =>
{
    var endpoint = builder.Configuration.GetValue<string>("MinIO__Endpoint") ?? "localhost:9000";
    var accessKey = builder.Configuration.GetValue<string>("MinIO__AccessKey") ?? "whyce";
    var secretKey = builder.Configuration.GetValue<string>("MinIO__SecretKey") ?? "whycepassword";
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
builder.Services.AddControllers();

var app = builder.Build();

// Metrics middleware — before everything else
app.UseMiddleware<MetricsMiddleware>();
app.UseRouting();
app.MapControllers();

// Prometheus /metrics endpoint
app.MapMetrics();

app.Run();

// --- In-Memory Adapters (Phase 1 Sandbox) ---

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

internal sealed class InMemoryEventStore : IEventStore
{
    private readonly Dictionary<Guid, List<object>> _store = new();

    public Task<IReadOnlyList<object>> LoadEventsAsync(Guid aggregateId)
    {
        _store.TryGetValue(aggregateId, out var events);
        return Task.FromResult<IReadOnlyList<object>>(events?.AsReadOnly() ?? (IReadOnlyList<object>)[]);
    }

    public Task AppendEventsAsync(Guid aggregateId, IReadOnlyList<object> events, int expectedVersion)
    {
        if (!_store.ContainsKey(aggregateId))
            _store[aggregateId] = new List<object>();
        _store[aggregateId].AddRange(events);
        return Task.CompletedTask;
    }
}

internal sealed class InMemoryChainAnchor : IChainAnchor
{
    private readonly List<ChainBlock> _blocks = new();

    public Task<ChainBlock> AnchorAsync(Guid correlationId, IReadOnlyList<object> events, string decisionHash)
    {
        var block = new ChainBlock(
            Guid.NewGuid(), correlationId, "event-hash", decisionHash,
            _blocks.Count > 0 ? _blocks[^1].BlockId.ToString() : "genesis",
            DateTimeOffset.UtcNow);
        _blocks.Add(block);
        return Task.FromResult(block);
    }
}

internal sealed class InMemoryOutbox : IOutbox
{
    public Task EnqueueAsync(Guid correlationId, IReadOnlyList<object> events)
    {
        return Task.CompletedTask;
    }
}

internal sealed class AllowAllPolicyEvaluator : IPolicyEvaluator
{
    public Task<PolicyDecision> EvaluateAsync(string policyId, object command, PolicyContext policyContext)
    {
        return Task.FromResult(new PolicyDecision(true, policyId, "sandbox-decision-hash", null));
    }
}

internal sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly HashSet<string> _keys = new();

    public Task<bool> ExistsAsync(string key) => Task.FromResult(_keys.Contains(key));
    public Task MarkAsync(string key) { _keys.Add(key); return Task.CompletedTask; }
}
