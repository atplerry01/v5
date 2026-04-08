using Whyce.Platform.Host.Composition.Orchestration.Workflow;
using Whyce.Projections.OrchestrationSystem.Workflow;
using Whyce.Runtime.EventFabric;
using FabricEventSchemaRegistry = Whyce.Runtime.EventFabric.EventSchemaRegistry;
using Whyce.Shared.Contracts.Events.OrchestrationSystem.Workflow;
using Whyce.Shared.Contracts.Runtime;
using Whyce.Shared.Kernel.Domain;
using Whycespace.Domain.OrchestrationSystem.Workflow.Execution;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Integration.Setup;
using Whycespace.Tests.Shared;
using Whyce.Platform.Host.Adapters;
using Whyce.Engines.T0U.WhyceChain.Engine;

namespace Whycespace.Tests.Integration.OrchestrationSystem.Workflow.Execution;

/// <summary>
/// Phase 1.6 Stabilization Gate — S0.1 closure.
///
/// The phase-1 audit sweep flagged WorkflowExecutionResumedEvent as an S0
/// activation gap: the event was emitted by the aggregate, exercised by unit
/// tests, and yet was not registered with the FabricEventSchemaRegistry, payload
/// mapper table, or projection registry. Any production resume would fail at
/// the fabric boundary.
///
/// This test exists specifically to prevent that regression. It drives the
/// REAL <see cref="EventFabric"/> with the REAL schema registry built by the
/// REAL <see cref="WorkflowExecutionBootstrap"/>, and proves the full chain:
///
///   Domain event → schema registration → payload mapper →
///     fabric persist → chain anchor → outbox enqueue →
///       envelope mapping → projection handler → read model updated
///
/// Per the new-rule candidate ACT-FABRIC-ROUNDTRIP-TEST-01, every event
/// registered with the schema registry must have at least one test that
/// round-trips it through the real fabric. This file is the canonical
/// realization for WorkflowExecutionResumedEvent.
/// </summary>
public sealed class WorkflowResumedEventFabricRoundTripTest
{
    private static readonly Guid ExecutionId = Guid.Parse("00000000-0000-0000-0000-000000000030");

    [Fact]
    public async Task Resume_Round_Trips_Through_Real_Fabric_Into_Read_Model()
    {
        // 1. Real schema registry, populated by the REAL bootstrap.
        var schemaRegistry = new FabricEventSchemaRegistry();
        var bootstrap = new WorkflowExecutionBootstrap();
        bootstrap.RegisterSchema(schemaRegistry);
        schemaRegistry.Lock();

        // 2. Real fabric, in-memory durability seams.
        var clock = new TestClock();
        var idGenerator = new TestIdGenerator();
        var eventStore = new InMemoryEventStore();
        var chainAnchor = new InMemoryChainAnchor(clock, idGenerator);
        var outbox = new InMemoryOutbox();

        var fabric = new EventFabric(
            new EventStoreService(eventStore),
            new ChainAnchorService(new WhyceChainEngine(), chainAnchor),
            new OutboxService(outbox),
            schemaRegistry,
            new TopicNameResolver(),
            clock);

        // 3. Real projection handler + store. The bootstrap's RegisterProjections
        //    contract is exercised by manually mirroring it (see assertion below).
        var projectionStore = new InMemoryWorkflowExecutionProjectionStore();
        var handler = new WorkflowExecutionProjectionHandler(projectionStore);

        // 4. Drive Started → StepCompleted → Failed → Resumed through the
        //    fabric. Each call must persist + anchor + enqueue without throwing.
        await ProcessAndProjectAsync(
            fabric, schemaRegistry, handler,
            new WorkflowExecutionStartedEvent(new AggregateId(ExecutionId), "wf", null, null),
            sequence: 0);

        await ProcessAndProjectAsync(
            fabric, schemaRegistry, handler,
            new WorkflowStepCompletedEvent(new AggregateId(ExecutionId), 0, "Validate", "h0", null, null),
            sequence: 1);

        await ProcessAndProjectAsync(
            fabric, schemaRegistry, handler,
            new WorkflowExecutionFailedEvent(new AggregateId(ExecutionId), "Charge", "card declined"),
            sequence: 2);

        // Sanity: read model is in Failed before resume.
        var preResume = await projectionStore.GetAsync(ExecutionId);
        Assert.NotNull(preResume);
        Assert.Equal("Failed", preResume!.Status);
        Assert.Equal("Charge", preResume.FailedStepName);
        Assert.Equal("card declined", preResume.FailureReason);

        // 5. The S0 path: WorkflowExecutionResumedEvent through the real fabric.
        var resumedDomain = new WorkflowExecutionResumedEvent(
            new AggregateId(ExecutionId), "Charge", "card declined");

        // 5a. Mapper-level proof: the registry MUST recognise the event and
        //     produce a WorkflowExecutionResumedEventSchema. If the bootstrap
        //     forgot to register the mapper, MapPayload returns the original
        //     domain event unchanged — this assertion catches that.
        var mapped = schemaRegistry.MapPayload("WorkflowExecutionResumedEvent", resumedDomain);
        Assert.IsType<WorkflowExecutionResumedEventSchema>(mapped);
        var mappedSchema = (WorkflowExecutionResumedEventSchema)mapped;
        Assert.Equal(ExecutionId, mappedSchema.AggregateId);
        Assert.Equal("Charge", mappedSchema.ResumedFromStepName);
        Assert.Equal("card declined", mappedSchema.PreviousFailureReason);

        // 5b. Fabric-level proof: ProcessAsync must persist + anchor + enqueue
        //     the resumed event without throwing.
        await ProcessAndProjectAsync(fabric, schemaRegistry, handler, resumedDomain, sequence: 3);

        // 6. Read model proof: resume transitions Failed → Running and clears
        //    the failure context.
        var postResume = await projectionStore.GetAsync(ExecutionId);
        Assert.NotNull(postResume);
        Assert.Equal("Running", postResume!.Status);
        Assert.Null(postResume.FailedStepName);
        Assert.Null(postResume.FailureReason);
        Assert.NotNull(postResume.LastEventId);

        // 7. Durability proof: every event landed in the event store, chain
        //    anchor, and outbox in order.
        var stored = eventStore.AllEvents(ExecutionId);
        Assert.Equal(4, stored.Count);
        Assert.IsType<WorkflowExecutionStartedEvent>(stored[0]);
        Assert.IsType<WorkflowStepCompletedEvent>(stored[1]);
        Assert.IsType<WorkflowExecutionFailedEvent>(stored[2]);
        Assert.IsType<WorkflowExecutionResumedEvent>(stored[3]);
        Assert.Equal(4, chainAnchor.Blocks.Count);
        Assert.Equal(4, outbox.Batches.Count);

        // 8. Topic resolution proof: the resumed event resolves through the
        //    canonical TopicNameResolver to the workflow execution events
        //    topic. Same routing as the other lifecycle events.
        Assert.All(outbox.Batches, b =>
            Assert.Equal("whyce.orchestration-system.workflow.execution.events", b.Topic));
    }

    [Fact]
    public void Bootstrap_Registers_Resumed_Event_Projection_Handler()
    {
        // Independent registration witness: build a ProjectionRegistry and run
        // RegisterProjections through the real bootstrap. The registry must
        // contain a handler for "WorkflowExecutionResumedEvent". This is the
        // static half of ACT-FABRIC-ROUNDTRIP-TEST-01.
        var projectionRegistry = new Whyce.Runtime.Projection.ProjectionRegistry();
        var bootstrap = new WorkflowExecutionBootstrap();
        var store = new InMemoryWorkflowExecutionProjectionStore();
        var handler = new WorkflowExecutionProjectionHandler(store);
        var serviceProvider = new SingleServiceProvider(typeof(WorkflowExecutionProjectionHandler), handler);

        bootstrap.RegisterProjections(serviceProvider, projectionRegistry);

        var handlers = projectionRegistry.ResolveHandlers("WorkflowExecutionResumedEvent");
        Assert.NotEmpty(handlers);
        Assert.Same(handler, handlers[0]);
    }

    private static async Task ProcessAndProjectAsync(
        EventFabric fabric,
        FabricEventSchemaRegistry schemaRegistry,
        WorkflowExecutionProjectionHandler handler,
        object domainEvent,
        int sequence)
    {
        var ctx = new CommandContext
        {
            CorrelationId = Guid.Parse("00000000-0000-0000-0000-000000000100"),
            CausationId = Guid.Parse("00000000-0000-0000-0000-000000000101"),
            CommandId = Guid.Parse("00000000-0000-0000-0000-000000000102"),
            TenantId = "test-tenant",
            ActorId = "test-actor",
            AggregateId = ExecutionId,
            PolicyId = "default",
            Classification = "orchestration-system",
            Context = "workflow",
            Domain = "execution"
        };

        await fabric.ProcessAsync(new[] { domainEvent }, ctx);

        // Mirror the canonical Kafka consumer path: build the same envelope the
        // fabric built (using the same registry mapping), then dispatch to the
        // projection handler. In production this happens inside
        // GenericKafkaProjectionConsumerWorker — here we synthesise the same
        // envelope shape so the test can witness the read-model effect without
        // a Kafka broker.
        var eventTypeName = domainEvent.GetType().Name;
        var schemaEntry = schemaRegistry.Resolve(eventTypeName);
        var envelope = new EventEnvelope
        {
            EventId = EventEnvelope.GenerateDeterministicId(ctx.CorrelationId, eventTypeName, sequence),
            AggregateId = ExecutionId,
            CorrelationId = ctx.CorrelationId,
            EventType = eventTypeName,
            EventName = schemaEntry.EventName,
            EventVersion = schemaEntry.Version,
            SchemaHash = schemaEntry.SchemaHash,
            Payload = schemaRegistry.MapPayload(eventTypeName, domainEvent),
            ExecutionHash = "test-execution-hash",
            PolicyHash = "test-policy-hash",
            Timestamp = DateTimeOffset.UnixEpoch,
            SequenceNumber = sequence,
            Classification = ctx.Classification,
            Context = ctx.Context,
            Domain = ctx.Domain
        };

        await handler.HandleAsync(envelope);
    }

    private sealed class SingleServiceProvider : IServiceProvider
    {
        private readonly Type _type;
        private readonly object _instance;
        public SingleServiceProvider(Type type, object instance)
        {
            _type = type;
            _instance = instance;
        }
        public object? GetService(Type serviceType) =>
            serviceType == _type ? _instance : null;
    }
}
