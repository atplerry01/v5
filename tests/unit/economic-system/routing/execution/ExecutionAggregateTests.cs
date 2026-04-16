using Whycespace.Domain.EconomicSystem.Routing.Execution;
using Whycespace.Domain.EconomicSystem.Routing.Path;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Routing.Execution;

public sealed class ExecutionAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp StartAt = new(new DateTimeOffset(2026, 4, 7, 12, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp TerminalAt = new(StartAt.Value.AddMinutes(5));

    private static (ExecutionAggregate aggregate, ExecutionId executionId, PathId pathId) NewStarted(string seed)
    {
        var executionId = new ExecutionId(IdGen.Generate($"ExecutionAggregateTests:{seed}:execution"));
        var pathId = new PathId(IdGen.Generate($"ExecutionAggregateTests:{seed}:path"));
        var aggregate = ExecutionAggregate.Start(executionId, pathId, StartAt);
        return (aggregate, executionId, pathId);
    }

    [Fact]
    public void Start_WithValidPath_RaisesStartedEvent()
    {
        var (aggregate, executionId, pathId) = NewStarted("Start");

        Assert.Equal(ExecutionStatus.Started, aggregate.Status);
        Assert.Equal(executionId, aggregate.ExecutionId);
        Assert.Equal(pathId, aggregate.PathId);
        Assert.Null(aggregate.TerminalAt);

        var evt = Assert.IsType<ExecutionStartedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(executionId, evt.ExecutionId);
        Assert.Equal(pathId, evt.PathId);
    }

    [Fact]
    public void Complete_FromStarted_TransitionsToCompleted()
    {
        var (aggregate, _, _) = NewStarted("Complete");
        aggregate.ClearDomainEvents();

        aggregate.Complete(TerminalAt);

        Assert.Equal(ExecutionStatus.Completed, aggregate.Status);
        Assert.Equal(TerminalAt, aggregate.TerminalAt);
        Assert.Null(aggregate.TerminalReason);
        Assert.IsType<ExecutionCompletedEvent>(Assert.Single(aggregate.DomainEvents));
    }

    [Fact]
    public void Fail_FromStarted_CapturesReasonAndTransitionsToFailed()
    {
        var (aggregate, _, _) = NewStarted("Fail");
        aggregate.ClearDomainEvents();

        aggregate.Fail("upstream timeout", TerminalAt);

        Assert.Equal(ExecutionStatus.Failed, aggregate.Status);
        Assert.Equal(TerminalAt, aggregate.TerminalAt);
        Assert.Equal("upstream timeout", aggregate.TerminalReason);

        var evt = Assert.IsType<ExecutionFailedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal("upstream timeout", evt.Reason);
    }

    [Fact]
    public void Abort_FromStarted_CapturesReasonAndTransitionsToAborted()
    {
        var (aggregate, _, _) = NewStarted("Abort");
        aggregate.ClearDomainEvents();

        aggregate.Abort("operator request", TerminalAt);

        Assert.Equal(ExecutionStatus.Aborted, aggregate.Status);
        Assert.Equal(TerminalAt, aggregate.TerminalAt);
        Assert.Equal("operator request", aggregate.TerminalReason);

        var evt = Assert.IsType<ExecutionAbortedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal("operator request", evt.Reason);
    }

    [Theory]
    [InlineData("Completed", "Complete")]
    [InlineData("Completed", "Fail")]
    [InlineData("Completed", "Abort")]
    [InlineData("Failed", "Complete")]
    [InlineData("Failed", "Fail")]
    [InlineData("Failed", "Abort")]
    [InlineData("Aborted", "Complete")]
    [InlineData("Aborted", "Fail")]
    [InlineData("Aborted", "Abort")]
    public void Transition_FromTerminalState_Throws(string terminalState, string attemptedTransition)
    {
        var (aggregate, _, _) = NewStarted($"Terminal_{terminalState}_{attemptedTransition}");

        switch (terminalState)
        {
            case "Completed": aggregate.Complete(TerminalAt); break;
            case "Failed": aggregate.Fail("x", TerminalAt); break;
            case "Aborted": aggregate.Abort("x", TerminalAt); break;
        }

        Action act = attemptedTransition switch
        {
            "Complete" => () => aggregate.Complete(TerminalAt),
            "Fail" => () => aggregate.Fail("again", TerminalAt),
            "Abort" => () => aggregate.Abort("again", TerminalAt),
            _ => throw new InvalidOperationException()
        };

        Assert.Throws<DomainException>(act);
    }
}
