using Whycespace.Domain.ControlSystem.Scheduling.ScheduleControl;
using Whycespace.Shared.Contracts.Control.Scheduling.ScheduleControl;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Scheduling.ScheduleControl;

public sealed class DefineScheduleControlHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DefineScheduleControlCommand cmd)
            return Task.CompletedTask;

        var aggregate = ScheduleControlAggregate.Define(
            new ScheduleControlId(cmd.ScheduleId.ToString("N").PadRight(64, '0')),
            cmd.JobDefinitionId,
            cmd.TriggerExpression);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
