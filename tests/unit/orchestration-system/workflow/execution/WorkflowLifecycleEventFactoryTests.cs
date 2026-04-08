using NSubstitute;
using Whyce.Engines.T1M.Lifecycle;
using Whyce.Shared.Contracts.EventFabric;
using Whycespace.Domain.OrchestrationSystem.Workflow.Execution;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Tests.Unit.OrchestrationSystem.Workflow.Execution;

/// <summary>
/// phase1.6-S1.2 (E-LIFECYCLE-FACTORY-CALL-SITE-01): the resume lifecycle
/// transition is no longer a command on <see cref="WorkflowExecutionAggregate"/>.
/// <see cref="WorkflowLifecycleEventFactory.Resumed"/> is the canonical and
/// only construction site for <see cref="WorkflowExecutionResumedEvent"/>,
/// and it owns the precondition (Failed-only) the aggregate previously
/// guarded.
///
/// These tests pin that contract: precondition enforcement, failure-context
/// propagation, and the explicit guarantee that the factory does NOT mutate
/// the aggregate it inspects (the runtime persist pipeline is the sole state
/// mutator, via Apply on replay).
/// </summary>
public sealed class WorkflowLifecycleEventFactoryTests
{
    private static WorkflowExecutionId NewId() =>
        new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    private static WorkflowLifecycleEventFactory NewFactory() =>
        new(Substitute.For<IPayloadTypeRegistry>());

    [Fact]
    public void Resumed_FromFailed_BuildsEventCarryingFailureContext()
    {
        var factory = NewFactory();
        var aggregate = WorkflowExecutionAggregate.Start(NewId(), "wf");
        aggregate.Fail("Validate", "boom");

        var evt = factory.Resumed(aggregate);

        Assert.Equal(NewId().Value, evt.AggregateId.Value);
        Assert.Equal("Validate", evt.ResumedFromStepName);
        Assert.Equal("boom", evt.PreviousFailureReason);
    }

    [Fact]
    public void Resumed_FromFailed_DoesNotMutateAggregate()
    {
        var factory = NewFactory();
        var aggregate = WorkflowExecutionAggregate.Start(NewId(), "wf");
        aggregate.Fail("Validate", "boom");
        var domainEventsBefore = aggregate.DomainEvents.Count;
        var statusBefore = aggregate.Status;

        _ = factory.Resumed(aggregate);

        // The factory only constructs and returns — the aggregate's
        // event list is unchanged and its status remains Failed until the
        // resume event is replayed back through Apply by the persist pipeline.
        Assert.Equal(domainEventsBefore, aggregate.DomainEvents.Count);
        Assert.Equal(statusBefore, aggregate.Status);
        Assert.Equal(WorkflowExecutionStatus.Failed, aggregate.Status);
    }

    [Fact]
    public void Resumed_FromRunning_Throws()
    {
        var factory = NewFactory();
        var aggregate = WorkflowExecutionAggregate.Start(NewId(), "wf");

        Assert.Throws<DomainInvariantViolationException>(() => factory.Resumed(aggregate));
    }

    [Fact]
    public void Resumed_FromCompleted_Throws()
    {
        var factory = NewFactory();
        var aggregate = WorkflowExecutionAggregate.Start(NewId(), "wf");
        aggregate.Complete("h");

        Assert.Throws<DomainInvariantViolationException>(() => factory.Resumed(aggregate));
    }

    [Fact]
    public void Resumed_NullAggregate_Throws()
    {
        var factory = NewFactory();
        Assert.Throws<ArgumentNullException>(() => factory.Resumed(null!));
    }
}
