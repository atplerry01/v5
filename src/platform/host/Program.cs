using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Minio;
using Prometheus;
using Whyce.Engines.T0U.WhyceId;
using Whyce.Engines.T0U.WhycePolicy;
using Whyce.Engines.T1M.StepExecutor;
using Whyce.Engines.T1M.Steps.Todo;
using Whyce.Engines.T1M.WorkflowEngine;
using Whyce.Engines.T2E.Operational.Todo;
using Whyce.Platform.Api.Extensions;
using Whyce.Platform.Api.Health;
using Whyce.Platform.Host.Health;
using Whyce.Projections.OperationalSystem.Sandbox.Todo;
using Whyce.Runtime.Observability;
using Whyce.Runtime.ControlPlane;
using Whyce.Runtime.Pipeline;
using RuntimeMiddleware = Whyce.Runtime.Pipeline.IMiddleware;
using Whyce.Shared.Contracts.Application.Todo;
using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.Events.Todo;
using Whyce.Shared.Contracts.Infrastructure.Chain;
using Whyce.Shared.Contracts.Infrastructure.Health;
using Whyce.Shared.Contracts.Infrastructure.Messaging;
using Whyce.Shared.Contracts.Infrastructure.Persistence;
using Whyce.Shared.Contracts.Infrastructure.Policy;
using Whyce.Shared.Contracts.Infrastructure.Projection;
using Whyce.Shared.Contracts.Runtime;
using Whyce.Shared.Kernel.Domain;
using Whyce.Systems.Downstream.OperationalSystem.Sandbox.Todo;

var builder = WebApplication.CreateBuilder(args);

// IClock — deterministic time source (inject fixed clock for replay/test)
builder.Services.AddSingleton<IClock, SystemClock>();

// IIdGenerator — deterministic ID generation (SHA256-based)
builder.Services.AddSingleton<IIdGenerator, DeterministicIdGenerator>();

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

// Engines — T0U (identity + policy, pre-execution)
builder.Services.AddTransient<WhyceIdEngine>();
builder.Services.AddTransient<WhycePolicyEngine>();

// Engines — T1M (workflow orchestration)
builder.Services.AddTransient<WorkflowStepExecutor>();
builder.Services.AddSingleton<IWorkflowEngine, T1MWorkflowEngine>();

// Engines — T1M workflow steps
builder.Services.AddTransient<ValidateIntentStep>();
builder.Services.AddTransient<CreateTodoStep>();
builder.Services.AddTransient<EmitCompletionStep>();

// Engines — T2E (domain execution)
builder.Services.AddTransient<TodoEngine>();

// Infrastructure adapters (in-memory for Phase 1 sandbox)
builder.Services.AddSingleton<IEventStore, InMemoryEventStore>();
builder.Services.AddSingleton<IChainAnchor>(sp =>
    new InMemoryChainAnchor(sp.GetRequiredService<IClock>()));
builder.Services.AddSingleton<IRedisClient, InMemoryRedisClient>();
builder.Services.AddSingleton<IPolicyEvaluator, AllowAllPolicyEvaluator>();
builder.Services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();

// Projection layer — Todo
builder.Services.AddSingleton<TodoProjectionHandler>();
builder.Services.AddSingleton<TodoProjectionConsumer>();

// Outbox wired to projection consumers (simulates Kafka relay for Phase 1)
builder.Services.AddSingleton<IOutbox>(sp =>
    new InMemoryOutbox(sp.GetRequiredService<TodoProjectionConsumer>()));

// Middleware pipeline (order matters: context guard → policy → idempotency)
// Policy MUST execute BEFORE idempotency per canonical flow §4
builder.Services.AddSingleton<RuntimeMiddleware, ContextGuardMiddleware>();
builder.Services.AddSingleton<RuntimeMiddleware>(sp =>
    new PolicyMiddleware(
        sp.GetRequiredService<WhyceIdEngine>(),
        sp.GetRequiredService<WhycePolicyEngine>()));
builder.Services.AddSingleton<RuntimeMiddleware>(sp =>
    new IdempotencyMiddleware(sp.GetRequiredService<IIdempotencyStore>()));

// Workflow registry — bind workflow names to step types
builder.Services.AddSingleton<IWorkflowRegistry>(sp =>
{
    var registry = new Whyce.Runtime.Workflow.WorkflowRegistry();
    registry.Register("todo.lifecycle.create", new[]
    {
        typeof(ValidateIntentStep),
        typeof(CreateTodoStep),
        typeof(EmitCompletionStep)
    });
    return registry;
});

// Workflow dispatcher — systems entry point for workflow execution
builder.Services.AddSingleton<IWorkflowDispatcher, Whyce.Systems.Midstream.Wss.WorkflowDispatcher>();

// Runtime control plane — single entry point for all command execution
builder.Services.AddSingleton<IRuntimeControlPlane, RuntimeControlPlane>();

// Command dispatcher (pure router) + system intent dispatcher
builder.Services.AddSingleton<ICommandDispatcher, RuntimeCommandDispatcher>();
builder.Services.AddSingleton<ISystemIntentDispatcher, SystemIntentDispatcher>();

// Systems.Downstream — Todo intent handler
builder.Services.AddTransient<ITodoIntentHandler, TodoIntentHandler>();

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
builder.Services.AddWhyceSwagger();
builder.Services.AddControllers()
    .AddApplicationPart(typeof(Whyce.Platform.Api.Controllers.HealthController).Assembly);

var app = builder.Build();

// Metrics middleware — before everything else
app.UseMiddleware<MetricsMiddleware>();
app.UseRouting();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.DocumentTitle = "Whycespace — Phase 1 Sandbox";
    options.SwaggerEndpoint("/swagger/operational/swagger.json", "Operational API");
    options.SwaggerEndpoint("/swagger/infrastructure/swagger.json", "Infrastructure API");
});

app.MapControllers();

// Prometheus /metrics endpoint
app.MapMetrics();

app.Run();

// --- In-Memory Adapters (Phase 1 Sandbox) ---

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
    private readonly IClock _clock;
    private readonly List<ChainBlock> _blocks = new();

    public InMemoryChainAnchor(IClock clock)
    {
        _clock = clock;
    }

    public Task<ChainBlock> AnchorAsync(Guid correlationId, IReadOnlyList<object> events, string decisionHash)
    {
        var previousBlockHash = _blocks.Count > 0 ? _blocks[^1].BlockId.ToString() : "genesis";
        var eventHash = ComputeEventHash(events);
        var blockId = ComputeBlockId(previousBlockHash, eventHash, decisionHash);

        var block = new ChainBlock(
            blockId, correlationId, eventHash, decisionHash,
            previousBlockHash, _clock.UtcNow);
        _blocks.Add(block);
        return Task.FromResult(block);
    }

    private static string ComputeEventHash(IReadOnlyList<object> events)
    {
        var payload = JsonSerializer.Serialize(events);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexStringLower(hash);
    }

    private static Guid ComputeBlockId(string previousHash, string eventHash, string decisionHash)
    {
        var seed = $"{previousHash}:{eventHash}:{decisionHash}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        return new Guid(hash.AsSpan(0, 16));
    }
}

internal sealed class InMemoryRedisClient : IRedisClient
{
    private readonly ConcurrentDictionary<string, object> _store = new();

    public Task SetAsync<T>(string key, T value)
    {
        _store[key] = value!;
        return Task.CompletedTask;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        return Task.FromResult(_store.TryGetValue(key, out var value) ? (T)value : default);
    }
}

internal sealed class InMemoryOutbox : IOutbox
{
    private readonly IEventConsumer _consumer;
    private readonly HashSet<string> _processedKeys = new();

    public InMemoryOutbox(IEventConsumer consumer)
    {
        _consumer = consumer;
    }

    public async Task EnqueueAsync(Guid correlationId, IReadOnlyList<object> events)
    {
        for (var i = 0; i < events.Count; i++)
        {
            var idempotencyKey = ComputeIdempotencyKey(correlationId, events[i], i);
            if (!_processedKeys.Add(idempotencyKey))
                continue;

            var schema = MapToSchema(events[i]);
            if (schema is not null)
            {
                await _consumer.ConsumeAsync(schema);
            }
        }
    }

    private static string ComputeIdempotencyKey(Guid correlationId, object @event, int sequenceNumber)
    {
        var payload = JsonSerializer.Serialize(@event);
        var seed = $"{correlationId}:{payload}:{sequenceNumber}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        return Convert.ToHexStringLower(hash);
    }

    private static object? MapToSchema(object domainEvent)
    {
        return domainEvent switch
        {
            Whycespace.Domain.OperationalSystem.Sandbox.Todo.TodoCreatedEvent e
                => new TodoCreatedEventSchema(e.AggregateId.Value, e.Title),
            Whycespace.Domain.OperationalSystem.Sandbox.Todo.TodoUpdatedEvent e
                => new TodoUpdatedEventSchema(e.AggregateId.Value, e.Title),
            Whycespace.Domain.OperationalSystem.Sandbox.Todo.TodoCompletedEvent e
                => new TodoCompletedEventSchema(e.AggregateId.Value),
            _ => null
        };
    }
}

internal sealed class AllowAllPolicyEvaluator : IPolicyEvaluator
{
    public Task<Whyce.Shared.Contracts.Infrastructure.Policy.PolicyDecision> EvaluateAsync(string policyId, object command, PolicyContext policyContext)
    {
        return Task.FromResult(new Whyce.Shared.Contracts.Infrastructure.Policy.PolicyDecision(true, policyId, "sandbox-decision-hash", null));
    }
}

internal sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly HashSet<string> _keys = new();

    public Task<bool> ExistsAsync(string key) => Task.FromResult(_keys.Contains(key));
    public Task MarkAsync(string key) { _keys.Add(key); return Task.CompletedTask; }
}
