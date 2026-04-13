using System.Text.Json;
using Whyce.Engines.T1M.Lifecycle;
using Whyce.Runtime.EventFabric;
using Whyce.Shared.Contracts.EventFabric;
using Whyce.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Domain.OrchestrationSystem.Workflow.Execution;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Tests.Integration.Orchestration.Workflow.Execution;

/// <summary>
/// H10 strict payload-type rehydration coverage. Asserts that the
/// PayloadTypeRegistry round-trips CLR types and that the
/// WorkflowExecutionReplayService refuses to leak JsonElement past the replay
/// boundary when a discriminator is missing or unresolvable.
/// </summary>
public sealed class WorkflowPayloadTypeTests
{
    public sealed record OrderPayload(string OrderId, decimal Total);
    public sealed record StepOutput(string Confirmation);

    [Fact]
    public void Registry_Register_And_Resolve_RoundTrip()
    {
        var registry = new PayloadTypeRegistry();
        registry.Register<OrderPayload>();

        Assert.True(registry.TryGetName(typeof(OrderPayload), out var name));
        Assert.Equal(typeof(OrderPayload).FullName, name);
        Assert.Equal(typeof(OrderPayload), registry.Resolve(name!));
    }

    [Fact]
    public void Registry_Resolve_UnknownType_Throws()
    {
        var registry = new PayloadTypeRegistry();
        Assert.Throws<InvalidOperationException>(() => registry.Resolve("Some.Unknown.Type"));
    }

    [Fact]
    public void Registry_Lock_PreventsFurtherRegistration()
    {
        var registry = new PayloadTypeRegistry();
        registry.Register<OrderPayload>();
        registry.Lock();
        Assert.Throws<InvalidOperationException>(() => registry.Register<StepOutput>());
    }

    [Fact]
    public async Task Replay_Rehydrates_JsonElement_To_TypedClr()
    {
        var registry = new PayloadTypeRegistry();
        registry.Register<OrderPayload>();
        registry.Register<StepOutput>();
        registry.Lock();

        var executionId = Guid.Parse("00000000-0000-0000-0000-000000000010");
        var payloadJson = JsonSerializer.SerializeToElement(new OrderPayload("ord-1", 42.50m));
        var outputJson = JsonSerializer.SerializeToElement(new StepOutput("ack-1"));

        var store = new StubEventStore(new object[]
        {
            new WorkflowExecutionStartedEvent(
                new AggregateId(executionId), "wf", payloadJson, typeof(OrderPayload).FullName),
            new WorkflowStepCompletedEvent(
                new AggregateId(executionId), 0, "Confirm", "h0", outputJson, typeof(StepOutput).FullName),
            new WorkflowExecutionCompletedEvent(new AggregateId(executionId), "h-final"),
        });

        var service = new WorkflowExecutionReplayService(store, registry, new WorkflowLifecycleEventFactory(registry));
        var state = await service.ReplayAsync(executionId);

        Assert.NotNull(state);
        var payload = Assert.IsType<OrderPayload>(state!.Payload);
        Assert.Equal("ord-1", payload.OrderId);
        Assert.Equal(42.50m, payload.Total);

        var output = Assert.IsType<StepOutput>(state.StepOutputs["Confirm"]);
        Assert.Equal("ack-1", output.Confirmation);
    }

    [Fact]
    public async Task Replay_JsonElement_Without_TypeDiscriminator_Throws()
    {
        var registry = new PayloadTypeRegistry();
        registry.Lock();

        var executionId = Guid.Parse("00000000-0000-0000-0000-000000000011");
        var payloadJson = JsonSerializer.SerializeToElement(new { anything = 1 });

        var store = new StubEventStore(new object[]
        {
            new WorkflowExecutionStartedEvent(
                new AggregateId(executionId), "wf", payloadJson, PayloadType: null),
        });

        var service = new WorkflowExecutionReplayService(store, registry, new WorkflowLifecycleEventFactory(registry));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ReplayAsync(executionId));
    }

    [Fact]
    public async Task Replay_Unknown_TypeDiscriminator_Throws()
    {
        var registry = new PayloadTypeRegistry();
        registry.Lock();

        var executionId = Guid.Parse("00000000-0000-0000-0000-000000000012");
        var payloadJson = JsonSerializer.SerializeToElement(new { anything = 1 });

        var store = new StubEventStore(new object[]
        {
            new WorkflowExecutionStartedEvent(
                new AggregateId(executionId), "wf", payloadJson, "Not.A.Real.Type"),
        });

        var service = new WorkflowExecutionReplayService(store, registry, new WorkflowLifecycleEventFactory(registry));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ReplayAsync(executionId));
    }

    [Fact]
    public async Task Replay_InMemory_TypedPayload_Bypasses_Rehydration()
    {
        // In-process replay: the event store hands back the original CLR
        // reference. No discriminator is required because no JsonElement
        // crosses the replay boundary.
        var registry = new PayloadTypeRegistry();
        registry.Lock();

        var executionId = Guid.Parse("00000000-0000-0000-0000-000000000013");
        var payload = new OrderPayload("ord-2", 1m);

        var store = new StubEventStore(new object[]
        {
            new WorkflowExecutionStartedEvent(
                new AggregateId(executionId), "wf", payload, PayloadType: null),
            new WorkflowExecutionCompletedEvent(new AggregateId(executionId), "h"),
        });

        var service = new WorkflowExecutionReplayService(store, registry, new WorkflowLifecycleEventFactory(registry));
        var state = await service.ReplayAsync(executionId);

        Assert.NotNull(state);
        Assert.Same(payload, state!.Payload);
    }

    private sealed class StubEventStore : IEventStore
    {
        private readonly IReadOnlyList<object> _events;
        public StubEventStore(IReadOnlyList<object> events) { _events = events; }

        // phase1.5-S5.2.5 / TB-1: aligned with the post-TC-5 IEventStore
        // contract that carries CancellationToken end-to-end.
        public Task<IReadOnlyList<object>> LoadEventsAsync(
            Guid aggregateId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_events);

        public Task AppendEventsAsync(
            Guid aggregateId,
            IReadOnlyList<IEventEnvelope> envelopes,
            int expectedVersion,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
