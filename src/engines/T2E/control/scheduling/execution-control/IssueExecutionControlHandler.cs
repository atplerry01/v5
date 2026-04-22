using Whycespace.Domain.ControlSystem.Scheduling.ExecutionControl;
using Whycespace.Shared.Contracts.Control.Scheduling.ExecutionControl;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Scheduling.ExecutionControl;

public sealed class IssueExecutionControlHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not IssueExecutionControlCommand cmd)
            return Task.CompletedTask;

        var aggregate = ExecutionControlAggregate.Issue(
            new ExecutionControlId(cmd.ControlId.ToString("N").PadRight(64, '0')),
            cmd.JobInstanceId,
            Enum.Parse<ControlSignal>(cmd.Signal, ignoreCase: true),
            cmd.ActorId,
            cmd.IssuedAt);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
