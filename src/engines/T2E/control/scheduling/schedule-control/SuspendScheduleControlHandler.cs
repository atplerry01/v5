using Whycespace.Domain.ControlSystem.Scheduling.ScheduleControl;
using Whycespace.Shared.Contracts.Control.Scheduling.ScheduleControl;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Scheduling.ScheduleControl;

public sealed class SuspendScheduleControlHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not SuspendScheduleControlCommand)
            return;

        var aggregate = (ScheduleControlAggregate)await context.LoadAggregateAsync(typeof(ScheduleControlAggregate));
        aggregate.Suspend();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
