using Whyce.Engines.T2E.Operational.Todo;
using Whyce.Runtime.Pipeline;
using Whyce.Shared.Contracts.Application.Todo;
using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.Infrastructure.Chain;
using Whyce.Shared.Contracts.Infrastructure.Messaging;
using Whyce.Shared.Contracts.Infrastructure.Persistence;
using Whyce.Shared.Contracts.Infrastructure.Policy;
using Whyce.Shared.Contracts.Infrastructure.Projection;
using Whyce.Shared.Contracts.Runtime;
using Whyce.Systems.Downstream.OperationalSystem.Sandbox.Todo;
using Whyce.Projections.OperationalSystem.Sandbox.Todo;
using Whyce.Shared.Kernel.Domain;
using Whycespace.Domain.OperationalSystem.Sandbox.Todo;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Tests.Integration.OperationalSystem.Sandbox.Todo;

public sealed class TodoPipelineTests
{
    private readonly InMemoryEventStore _eventStore = new();
    private readonly InMemoryChainAnchor _chainAnchor = new();
    private readonly InMemoryOutbox _outbox = new();
    private readonly AllowAllPolicyEvaluator _policyEvaluator = new();

    private RuntimeCommandDispatcher BuildDispatcher()
    {
        var registry = new EngineRegistry();
        registry.Register<CreateTodoCommand, TodoEngine>();
        registry.Register<UpdateTodoCommand, TodoEngine>();
        registry.Register<CompleteTodoCommand, TodoEngine>();

        var services = new MinimalServiceProvider();
        services.Register(typeof(TodoEngine), new TodoEngine());

        // Canonical order: ContextGuard → Policy → Idempotency
        var middlewares = new IMiddleware[]
        {
            new ContextGuardMiddleware(),
            new PolicyMiddleware(_policyEvaluator),
            new IdempotencyMiddleware(new InMemoryIdempotencyStore())
        };

        return new RuntimeCommandDispatcher(
            middlewares, registry, services, _eventStore, _chainAnchor, _outbox);
    }

    private static CommandContext CreateContext(Guid aggregateId) => new()
    {
        CorrelationId = Guid.NewGuid(),
        CausationId = Guid.NewGuid(),
        CommandId = Guid.NewGuid(),
        TenantId = "test-tenant",
        ActorId = "test-actor",
        AggregateId = aggregateId,
        PolicyId = "whyce-policy-default",
        Classification = "operational",
        Context = "sandbox",
        Domain = "todo"
    };

    [Fact]
    public async Task FullLifecycle_Create_Update_Complete()
    {
        var dispatcher = BuildDispatcher();
        var id = Guid.NewGuid();

        // Create
        var createResult = await dispatcher.DispatchAsync(
            new CreateTodoCommand(id, "Buy milk"), CreateContext(id));
        Assert.True(createResult.IsSuccess);
        Assert.Single(createResult.EmittedEvents);
        Assert.IsType<TodoCreatedEvent>(createResult.EmittedEvents[0]);

        // Update
        var updateResult = await dispatcher.DispatchAsync(
            new UpdateTodoCommand(id, "Buy almond milk"), CreateContext(id));
        Assert.True(updateResult.IsSuccess);
        Assert.IsType<TodoUpdatedEvent>(updateResult.EmittedEvents[0]);

        // Complete
        var completeResult = await dispatcher.DispatchAsync(
            new CompleteTodoCommand(id), CreateContext(id));
        Assert.True(completeResult.IsSuccess);
        Assert.IsType<TodoCompletedEvent>(completeResult.EmittedEvents[0]);

        // Verify event store
        var storedEvents = await _eventStore.LoadEventsAsync(id);
        Assert.Equal(3, storedEvents.Count);

        // Verify chain anchored
        Assert.Equal(3, _chainAnchor.Blocks.Count);

        // Verify outbox (Kafka)
        Assert.Equal(3, _outbox.EnqueuedBatches.Count);
    }

    [Fact]
    public async Task PolicyDenial_BlocksExecution()
    {
        _policyEvaluator.ShouldDeny = true;
        var dispatcher = BuildDispatcher();
        var id = Guid.NewGuid();

        var result = await dispatcher.DispatchAsync(
            new CreateTodoCommand(id, "Blocked"), CreateContext(id));

        Assert.False(result.IsSuccess);
        Assert.Contains("Policy denied", result.Error);
        Assert.Empty(await _eventStore.LoadEventsAsync(id));
    }

    [Fact]
    public async Task MissingContext_FailsGuard()
    {
        var dispatcher = BuildDispatcher();

        var badContext = new CommandContext
        {
            CorrelationId = Guid.Empty, // Invalid
            CausationId = Guid.NewGuid(),
            CommandId = Guid.NewGuid(),
            TenantId = "t",
            ActorId = "a",
            AggregateId = Guid.NewGuid(),
            PolicyId = "p",
            Classification = "operational",
            Context = "sandbox",
            Domain = "todo"
        };

        var result = await dispatcher.DispatchAsync(
            new CreateTodoCommand(Guid.NewGuid(), "Test"), badContext);

        Assert.False(result.IsSuccess);
        Assert.Contains("CorrelationId", result.Error);
    }

    [Fact]
    public async Task UpdateAfterComplete_FailsDomainRule()
    {
        var dispatcher = BuildDispatcher();
        var id = Guid.NewGuid();

        await dispatcher.DispatchAsync(new CreateTodoCommand(id, "Task"), CreateContext(id));
        await dispatcher.DispatchAsync(new CompleteTodoCommand(id), CreateContext(id));

        // This should fail at domain level
        await Assert.ThrowsAsync<DomainInvariantViolationException>(async () =>
            await dispatcher.DispatchAsync(new UpdateTodoCommand(id, "Nope"), CreateContext(id)));
    }
}

/// <summary>
/// E2E test: Systems.Downstream → Runtime → Engine → Domain → Events → Persist → Chain → Kafka → Projection
/// Validates the canonical WBSM v3.5 execution flow end-to-end.
/// </summary>
public sealed class TodoE2EPipelineTests
{
    [Fact]
    public async Task E2E_CreateTodo_FullCanonicalFlow()
    {
        // --- Infrastructure ---
        var eventStore = new InMemoryEventStore();
        var chainAnchor = new InMemoryChainAnchor();
        var redisClient = new TestRedisClient();
        var projectionHandler = new TodoProjectionHandler(redisClient);
        var projectionConsumer = new TodoProjectionConsumer(projectionHandler);
        var outbox = new ProjectionWiredOutbox(projectionConsumer);
        var policyEvaluator = new AllowAllPolicyEvaluator();
        var idGenerator = new TestIdGenerator();

        // --- Engine Registry ---
        var registry = new EngineRegistry();
        registry.Register<CreateTodoCommand, TodoEngine>();

        var services = new MinimalServiceProvider();
        services.Register(typeof(TodoEngine), new TodoEngine());

        // --- Middleware (canonical order: Guard → Policy → Idempotency) ---
        var middlewares = new IMiddleware[]
        {
            new ContextGuardMiddleware(),
            new PolicyMiddleware(policyEvaluator),
            new IdempotencyMiddleware(new InMemoryIdempotencyStore())
        };

        // --- Runtime Dispatcher ---
        var commandDispatcher = new RuntimeCommandDispatcher(
            middlewares, registry, services, eventStore, chainAnchor, outbox);

        var systemIntentDispatcher = new SystemIntentDispatcher(
            commandDispatcher, new TestClock(), idGenerator);

        // --- Systems.Downstream (Intent Handler) ---
        var intentHandler = new TodoIntentHandler(systemIntentDispatcher, idGenerator);

        // === EXECUTE: Full flow from Systems.Downstream ===
        var intent = new CreateTodoIntent("Buy groceries", "Weekly shopping", "user-42");
        var systemResult = await intentHandler.HandleAsync(intent);

        // 1. Systems.Downstream returned success
        Assert.True(systemResult.Success);
        Assert.NotNull(systemResult.TodoId);
        Assert.Equal("created", systemResult.Status);

        // 2. Events persisted to EventStore
        var storedEvents = await eventStore.LoadEventsAsync(systemResult.TodoId!.Value);
        Assert.Single(storedEvents);
        Assert.IsType<TodoCreatedEvent>(storedEvents[0]);

        // 3. Chain anchored
        Assert.Single(chainAnchor.Blocks);
        var block = chainAnchor.Blocks[0];
        Assert.NotEqual(Guid.Empty, block.BlockId);
        Assert.NotEmpty(block.DecisionHash);

        // 4. Outbox delivered to projection (simulates Kafka)
        Assert.Single(outbox.DeliveredBatches);

        // 5. Projection updated in Redis
        var readModel = await redisClient.GetAsync<TodoReadModel>($"todo:{systemResult.TodoId}");
        Assert.NotNull(readModel);
        Assert.Equal("Buy groceries", readModel!.Title);
        Assert.False(readModel.IsCompleted);
        Assert.Equal("active", readModel.Status);
    }

    [Fact]
    public async Task E2E_SystemsDownstream_RejectsInvalidInput()
    {
        var idGenerator = new TestIdGenerator();
        var intentHandler = new TodoIntentHandler(
            new NoOpDispatcher(), idGenerator);

        // Missing title
        var result1 = await intentHandler.HandleAsync(
            new CreateTodoIntent("", "desc", "user-1"));
        Assert.False(result1.Success);
        Assert.Contains("Title", result1.Error);

        // Missing userId
        var result2 = await intentHandler.HandleAsync(
            new CreateTodoIntent("Valid", "desc", ""));
        Assert.False(result2.Success);
        Assert.Contains("UserId", result2.Error);
    }

    [Fact]
    public async Task E2E_PolicyDenial_BlocksFullPipeline()
    {
        var eventStore = new InMemoryEventStore();
        var chainAnchor = new InMemoryChainAnchor();
        var outbox = new InMemoryOutbox();
        var policyEvaluator = new AllowAllPolicyEvaluator { ShouldDeny = true };
        var idGenerator = new TestIdGenerator();

        var registry = new EngineRegistry();
        registry.Register<CreateTodoCommand, TodoEngine>();

        var services = new MinimalServiceProvider();
        services.Register(typeof(TodoEngine), new TodoEngine());

        var middlewares = new IMiddleware[]
        {
            new ContextGuardMiddleware(),
            new PolicyMiddleware(policyEvaluator),
            new IdempotencyMiddleware(new InMemoryIdempotencyStore())
        };

        var commandDispatcher = new RuntimeCommandDispatcher(
            middlewares, registry, services, eventStore, chainAnchor, outbox);

        var systemIntentDispatcher = new SystemIntentDispatcher(
            commandDispatcher, new TestClock(), idGenerator);

        var intentHandler = new TodoIntentHandler(systemIntentDispatcher, idGenerator);

        var intent = new CreateTodoIntent("Blocked", "test", "user-1");
        var result = await intentHandler.HandleAsync(intent);

        Assert.False(result.Success);
        Assert.Contains("Policy denied", result.Error);
        Assert.Empty(chainAnchor.Blocks);
    }
}

#region Test Doubles

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
    public List<ChainBlock> Blocks { get; } = new();

    public Task<ChainBlock> AnchorAsync(Guid correlationId, IReadOnlyList<object> events, string decisionHash)
    {
        var block = new ChainBlock(
            Guid.NewGuid(), correlationId, "event-hash", decisionHash,
            Blocks.Count > 0 ? Blocks[^1].BlockId.ToString() : "genesis",
            DateTimeOffset.UtcNow);
        Blocks.Add(block);
        return Task.FromResult(block);
    }
}

internal sealed class InMemoryOutbox : IOutbox
{
    public List<(Guid CorrelationId, IReadOnlyList<object> Events, string Topic)> EnqueuedBatches { get; } = new();

    public Task EnqueueAsync(Guid correlationId, IReadOnlyList<object> events, string topic)
    {
        EnqueuedBatches.Add((correlationId, events, topic));
        return Task.CompletedTask;
    }
}

internal sealed class AllowAllPolicyEvaluator : IPolicyEvaluator
{
    public bool ShouldDeny { get; set; }

    public Task<PolicyDecision> EvaluateAsync(string policyId, object command, PolicyContext policyContext)
    {
        if (ShouldDeny)
            return Task.FromResult(new PolicyDecision(false, policyId, "", "Denied by test"));

        return Task.FromResult(new PolicyDecision(true, policyId, "test-decision-hash", null));
    }
}

internal sealed class MinimalServiceProvider : IServiceProvider
{
    private readonly Dictionary<Type, object> _services = new();

    public void Register(Type type, object instance) => _services[type] = instance;

    public object? GetService(Type serviceType) =>
        _services.GetValueOrDefault(serviceType);
}

internal sealed class TestClock : IClock
{
    public DateTimeOffset UtcNow => new(2026, 4, 5, 0, 0, 0, TimeSpan.Zero);
}

internal sealed class TestIdGenerator : IIdGenerator
{
    public Guid Generate(string seed)
    {
        var hash = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(seed));
        return new Guid(hash.AsSpan(0, 16));
    }
}

internal sealed class TestRedisClient : IRedisClient
{
    private readonly Dictionary<string, object> _store = new();

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

internal sealed class ProjectionWiredOutbox : IOutbox
{
    private readonly IEventConsumer _consumer;
    public List<(Guid CorrelationId, IReadOnlyList<object> Events, string Topic)> DeliveredBatches { get; } = new();

    public ProjectionWiredOutbox(IEventConsumer consumer)
    {
        _consumer = consumer;
    }

    public async Task EnqueueAsync(Guid correlationId, IReadOnlyList<object> events, string topic)
    {
        DeliveredBatches.Add((correlationId, events, topic));
        foreach (var e in events)
        {
            var schema = MapToSchema(e);
            if (schema is not null)
                await _consumer.ConsumeAsync(schema);
        }
    }

    private static object? MapToSchema(object domainEvent)
    {
        return domainEvent switch
        {
            TodoCreatedEvent e => new Whyce.Shared.Contracts.Events.Todo.TodoCreatedEventSchema(e.AggregateId.Value, e.Title),
            TodoUpdatedEvent e => new Whyce.Shared.Contracts.Events.Todo.TodoUpdatedEventSchema(e.AggregateId.Value, e.Title),
            TodoCompletedEvent e => new Whyce.Shared.Contracts.Events.Todo.TodoCompletedEventSchema(e.AggregateId.Value),
            _ => null
        };
    }
}

internal sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly HashSet<string> _keys = new();
    public Task<bool> ExistsAsync(string key) => Task.FromResult(_keys.Contains(key));
    public Task MarkAsync(string key) { _keys.Add(key); return Task.CompletedTask; }
}

internal sealed class NoOpDispatcher : ISystemIntentDispatcher
{
    public Task<CommandResult> DispatchAsync(object command, DomainRoute route) =>
        Task.FromResult(CommandResult.Failure("NoOp — should not reach dispatcher"));
}

#endregion
