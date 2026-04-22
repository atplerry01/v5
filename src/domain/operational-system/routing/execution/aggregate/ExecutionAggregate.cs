using Whycespace.Domain.OperationalSystem.Routing.Path;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OperationalSystem.Routing.Execution;

public sealed class ExecutionAggregate : AggregateRoot
{
    public ExecutionId ExecutionId { get; private set; }
    public PathId PathId { get; private set; }
    public ExecutionStatus Status { get; private set; }
    public Timestamp StartedAt { get; private set; }
    public Timestamp? TerminalAt { get; private set; }
    public string? TerminalReason { get; private set; }

    private ExecutionAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static ExecutionAggregate Start(
        ExecutionId executionId,
        PathId pathId,
        Timestamp startedAt)
    {
        if (pathId.Value == Guid.Empty)
            throw ExecutionErrors.InvalidPathReference();

        var aggregate = new ExecutionAggregate();
        aggregate.RaiseDomainEvent(new ExecutionStartedEvent(executionId, pathId, startedAt));
        return aggregate;
    }

    // ── Complete ─────────────────────────────────────────────────

    public void Complete(Timestamp completedAt)
    {
        var specification = new CanCompleteExecutionSpecification();
        if (!specification.IsSatisfiedBy(this))
            throw ExecutionErrors.InvalidStateTransition(Status, ExecutionStatus.Completed);

        RaiseDomainEvent(new ExecutionCompletedEvent(ExecutionId, completedAt));
    }

    // ── Fail ─────────────────────────────────────────────────────

    public void Fail(string reason, Timestamp failedAt)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw ExecutionErrors.ReasonMustBeProvided();

        if (Status != ExecutionStatus.Started)
            throw ExecutionErrors.InvalidStateTransition(Status, ExecutionStatus.Failed);

        RaiseDomainEvent(new ExecutionFailedEvent(ExecutionId, reason, failedAt));
    }

    // ── Abort ────────────────────────────────────────────────────

    public void Abort(string reason, Timestamp abortedAt)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw ExecutionErrors.ReasonMustBeProvided();

        var specification = new CanAbortExecutionSpecification();
        if (!specification.IsSatisfiedBy(this))
            throw ExecutionErrors.InvalidStateTransition(Status, ExecutionStatus.Aborted);

        RaiseDomainEvent(new ExecutionAbortedEvent(ExecutionId, reason, abortedAt));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ExecutionStartedEvent e:
                ExecutionId = e.ExecutionId;
                PathId = e.PathId;
                StartedAt = e.StartedAt;
                Status = ExecutionStatus.Started;
                break;

            case ExecutionCompletedEvent e:
                TerminalAt = e.CompletedAt;
                Status = ExecutionStatus.Completed;
                break;

            case ExecutionFailedEvent e:
                TerminalAt = e.FailedAt;
                TerminalReason = e.Reason;
                Status = ExecutionStatus.Failed;
                break;

            case ExecutionAbortedEvent e:
                TerminalAt = e.AbortedAt;
                TerminalReason = e.Reason;
                Status = ExecutionStatus.Aborted;
                break;
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (PathId.Value == Guid.Empty)
            throw ExecutionErrors.PathIdMustNotBeEmpty();

        if (TerminalAt.HasValue && TerminalAt.Value.Value < StartedAt.Value)
            throw ExecutionErrors.TerminalBeforeStart();
    }
}
