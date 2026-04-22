using Whycespace.Domain.ControlSystem.Scheduling.ExecutionControl;
using Whycespace.Shared.Contracts.Control.Scheduling.ExecutionControl;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Scheduling.ExecutionControl;

public sealed class RecordExecutionControlOutcomeHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RecordExecutionControlOutcomeCommand cmd)
            return;

        var aggregate = (ExecutionControlAggregate)await context.LoadAggregateAsync(typeof(ExecutionControlAggregate));
        aggregate.Apply(Enum.Parse<ControlSignalOutcome>(cmd.Outcome, ignoreCase: true));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
