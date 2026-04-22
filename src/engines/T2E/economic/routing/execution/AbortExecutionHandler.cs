using Whycespace.Domain.OperationalSystem.Routing.Execution;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Routing.Execution;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T2E.Economic.Routing.Execution;

public sealed class AbortExecutionHandler : IEngine
{
    private readonly IClock _clock;

    public AbortExecutionHandler(IClock clock) => _clock = clock;

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not AbortExecutionCommand cmd)
            return;

        var aggregate = (ExecutionAggregate)await context.LoadAggregateAsync(typeof(ExecutionAggregate));
        aggregate.Abort(cmd.Reason, new Timestamp(_clock.UtcNow));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
