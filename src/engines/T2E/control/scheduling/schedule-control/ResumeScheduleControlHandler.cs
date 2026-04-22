using Whycespace.Domain.ControlSystem.Scheduling.ScheduleControl;
using Whycespace.Shared.Contracts.Control.Scheduling.ScheduleControl;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Scheduling.ScheduleControl;

public sealed class ResumeScheduleControlHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ResumeScheduleControlCommand)
            return;

        var aggregate = (ScheduleControlAggregate)await context.LoadAggregateAsync(typeof(ScheduleControlAggregate));
        aggregate.Resume();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
