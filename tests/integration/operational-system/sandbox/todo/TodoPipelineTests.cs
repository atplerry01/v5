using Whyce.Engines.T2E.Operational.Todo;
using Whyce.Runtime.Pipeline;
using Whyce.Shared.Contracts.Application.Todo;
using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.Infrastructure.Chain;
using Whyce.Shared.Contracts.Infrastructure.Messaging;
using Whyce.Shared.Contracts.Infrastructure.Persistence;
using Whyce.Shared.Contracts.Infrastructure.Policy;
using Whyce.Shared.Contracts.Runtime;
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

        var middlewares = new IMiddleware[]
        {
            new ContextGuardMiddleware(),
            new PolicyMiddleware(_policyEvaluator)
        };

        return new RuntimeCommandDispatcher(
            middlewares, registry, services, _eventStore, _chainAnchor, _outbox);
    }

    private static CommandContext CreateContext(Guid aggregateId) => new()
    {
        CorrelationId = Guid.NewGuid(),
        CausationId = Guid.NewGuid(),
        TenantId = "test-tenant",
        ActorId = "test-actor",
        AggregateId = aggregateId,
        PolicyId = "whyce-policy-default"
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
            TenantId = "t",
            ActorId = "a",
            AggregateId = Guid.NewGuid(),
            PolicyId = "p"
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
    public List<(Guid CorrelationId, IReadOnlyList<object> Events)> EnqueuedBatches { get; } = new();

    public Task EnqueueAsync(Guid correlationId, IReadOnlyList<object> events)
    {
        EnqueuedBatches.Add((correlationId, events));
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

#endregion
