using NSubstitute;
using Whycespace.Engines.T1M.Lifecycle;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Domain.OrchestrationSystem.Workflow.Execution;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Tests.Unit.Orchestration.Workflow.Execution;

/// <summary>
/// Lifecycle, invariant, and replay-determinism tests for the
/// event-sourced <see cref="WorkflowExecutionAggregate"/>. Asserts that the
/// aggregate is the sole authority for workflow execution state and that
/// every transition is mediated by an event.
/// </summary>
public sealed class WorkflowExecutionAggregateTests
{
    private static WorkflowExecutionId NewId() => new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    [Fact]
    public void Start_RaisesStartedEvent_AndStatusBecomesRunning()
    {
        var aggregate = WorkflowExecutionAggregate.Start(NewId(), "OrderFulfillment");

        Assert.Equal(WorkflowExecutionStatus.Running, aggregate.Status);
        Assert.Equal("OrderFulfillment", aggregate.WorkflowName);
        Assert.Empty(aggregate.CompletedSteps);
        var raised = Assert.Single(aggregate.DomainEvents);
        Assert.IsType<WorkflowExecutionStartedEvent>(raised);
    }

    [Fact]
    public void Start_WithBlankWorkflowName_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() => WorkflowExecutionAggregate.Start(NewId(), "  "));
    }

    [Fact]
    public void FullLifecycle_Replay_ProducesCorrectState()
    {
        var aggregate = WorkflowExecutionAggregate.Start(NewId(), "OrderFulfillment");
        aggregate.CompleteStep(0, "Validate", "hash-0");
        aggregate.CompleteStep(1, "Reserve", "hash-1");
        aggregate.CompleteStep(2, "Charge", "hash-2");
        aggregate.Complete("hash-final");

        var history = aggregate.DomainEvents.ToList();
        var replayed = NewBareAggregate();
        replayed.LoadFromHistory(history);

        Assert.Equal(WorkflowExecutionStatus.Completed, replayed.Status);
        Assert.Equal("OrderFulfillment", replayed.WorkflowName);
        Assert.Equal(new[] { "Validate", "Reserve", "Charge" }, replayed.CompletedSteps);
        Assert.Equal("hash-final", replayed.ExecutionHash);
    }

    [Fact]
    public void Replay_IsDeterministic_AcrossInstances()
    {
        // phase1.6-S1.2: resume is no longer a command on the aggregate; the
        // resumed event comes from WorkflowLifecycleEventFactory.Resumed and
        // the aggregate consumes it via Apply on replay. Fixture mirrors the
        // production path: build pre-resume stream, factory-construct the
        // resume event, then replay-then-continue on a fresh aggregate.
        var factory = NewFactory();

        var pre = WorkflowExecutionAggregate.Start(NewId(), "OrderFulfillment");
        pre.CompleteStep(0, "Validate", "h0");
        pre.Fail("Validate", "boom");

        var resumedEvt = factory.Resumed(pre);

        var continuation = NewBareAggregate();
        continuation.LoadFromHistory(pre.DomainEvents.Concat(new[] { (object)resumedEvt }).ToList());
        continuation.CompleteStep(1, "Reserve", "h1");
        continuation.Complete("h-final");

        var history = pre.DomainEvents
            .Concat(new object[] { resumedEvt })
            .Concat(continuation.DomainEvents)
            .ToList();

        var a = NewBareAggregate();
        var b = NewBareAggregate();
        a.LoadFromHistory(history);
        b.LoadFromHistory(history);

        Assert.Equal(a.Status, b.Status);
        Assert.Equal(a.WorkflowName, b.WorkflowName);
        Assert.Equal(a.ExecutionHash, b.ExecutionHash);
        Assert.Equal(a.CompletedSteps, b.CompletedSteps);
        Assert.Equal(a.FailedStepName, b.FailedStepName);
        Assert.Equal(a.FailureReason, b.FailureReason);
    }

    [Fact]
    public void CompleteStep_BeforeStarted_Throws()
    {
        var aggregate = NewBareAggregate();
        Assert.Throws<DomainInvariantViolationException>(() => aggregate.CompleteStep(0, "Validate", "h"));
    }

    [Fact]
    public void Complete_BeforeStarted_Throws()
    {
        var aggregate = NewBareAggregate();
        Assert.Throws<DomainInvariantViolationException>(() => aggregate.Complete("h"));
    }

    [Fact]
    public void CompleteStep_AfterCompleted_Throws()
    {
        var aggregate = WorkflowExecutionAggregate.Start(NewId(), "wf");
        aggregate.Complete("h");
        Assert.Throws<DomainInvariantViolationException>(() => aggregate.CompleteStep(0, "Validate", "h"));
    }

    [Fact]
    public void CompleteStep_OutOfOrder_Throws()
    {
        var aggregate = WorkflowExecutionAggregate.Start(NewId(), "wf");
        Assert.Throws<DomainInvariantViolationException>(() => aggregate.CompleteStep(1, "Reserve", "h"));
    }

    // phase1.6-S1.2: precondition + happy-path tests for the resume transition
    // moved to WorkflowLifecycleEventFactoryTests, since the factory is the
    // canonical construction site after the aggregate-side Resume() command
    // was removed (E-LIFECYCLE-FACTORY-CALL-SITE-01).

    [Fact]
    public void Aggregate_IsSoleAuthority_NoExternalStateRequired()
    {
        // The aggregate exposes no setters and no construction path other than
        // the static factory and event replay. Reflection over public surface
        // confirms there are no public state mutators.
        var publicSetters = typeof(WorkflowExecutionAggregate)
            .GetProperties()
            .Where(p => p.SetMethod is { IsPublic: true })
            .ToList();
        Assert.Empty(publicSetters);
    }

    [Fact]
    public void WorkflowExecution_EndToEnd_Replay_Test()
    {
        // H9 closure end-to-end replay coverage. Drives a workflow through the
        // canonical lifecycle: Start → run several steps → Fail → Resume →
        // continue from the failed step → Complete. Then reconstructs the
        // aggregate from the resulting event stream alone and verifies that
        // status, completed steps, failure cleared, and final consistency all
        // match the in-memory aggregate state. Proves the event stream is
        // sufficient to fully rebuild workflow state with no external input.
        var factory = NewFactory();
        var pre = WorkflowExecutionAggregate.Start(NewId(), "OrderFulfillment");
        pre.CompleteStep(0, "Validate", "h0");
        pre.CompleteStep(1, "Reserve", "h1");
        pre.Fail("Charge", "card declined");

        Assert.Equal(WorkflowExecutionStatus.Failed, pre.Status);
        Assert.Equal("Charge", pre.FailedStepName);
        Assert.Equal("card declined", pre.FailureReason);

        // phase1.6-S1.2: factory constructs the resume event without mutating
        // the aggregate. Continuation aggregate is hydrated from the full
        // history including the resume event before issuing further commands.
        var resumedEvt = factory.Resumed(pre);

        var continuation = NewBareAggregate();
        continuation.LoadFromHistory(pre.DomainEvents.Concat(new[] { (object)resumedEvt }).ToList());
        Assert.Equal(WorkflowExecutionStatus.Running, continuation.Status);

        continuation.CompleteStep(2, "Charge", "h2");
        continuation.CompleteStep(3, "Ship", "h3");
        continuation.Complete("h-final");

        var history = pre.DomainEvents
            .Concat(new object[] { resumedEvt })
            .Concat(continuation.DomainEvents)
            .ToList();

        // Reload via replay only — no external state, no shortcuts.
        var replayed = NewBareAggregate();
        replayed.LoadFromHistory(history);

        // Status: Completed (final consistency)
        Assert.Equal(WorkflowExecutionStatus.Completed, replayed.Status);

        // Completed steps: all four, in order
        Assert.Equal(new[] { "Validate", "Reserve", "Charge", "Ship" }, replayed.CompletedSteps);

        // ExecutionHash: matches the final hash from Complete
        Assert.Equal("h-final", replayed.ExecutionHash);

        // Workflow name preserved across replay
        Assert.Equal("OrderFulfillment", replayed.WorkflowName);

        // Failure fields are still observable on the replayed aggregate (audit
        // trail) — Resume does not erase them, only transitions status. The
        // failure is "cleared" in the sense that Status moved past Failed and
        // the workflow ran to completion.
        Assert.Equal("Charge", replayed.FailedStepName);
        Assert.Equal("card declined", replayed.FailureReason);

        // Replay is deterministic: a second pass over the same history yields
        // the same state on a fresh aggregate.
        var replayedAgain = NewBareAggregate();
        replayedAgain.LoadFromHistory(history);
        Assert.Equal(replayed.Status, replayedAgain.Status);
        Assert.Equal(replayed.CompletedSteps, replayedAgain.CompletedSteps);
        Assert.Equal(replayed.ExecutionHash, replayedAgain.ExecutionHash);
    }

    private static WorkflowExecutionAggregate NewBareAggregate() =>
        (WorkflowExecutionAggregate)System.Activator.CreateInstance(
            typeof(WorkflowExecutionAggregate), nonPublic: true)!;

    // phase1.6-S1.2: factory needs IPayloadTypeRegistry for Started/StepCompleted
    // discriminator stamping. Resumed() does not consult the registry, so a
    // bare NSubstitute stub is sufficient for these tests.
    private static WorkflowLifecycleEventFactory NewFactory() =>
        new(Substitute.For<IPayloadTypeRegistry>());
}
