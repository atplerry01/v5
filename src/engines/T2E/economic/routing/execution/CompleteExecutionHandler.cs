using Whycespace.Domain.OperationalSystem.Routing.Execution;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Routing.Execution;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T2E.Economic.Routing.Execution;

public sealed class CompleteExecutionHandler : IEngine
{
    private readonly IClock _clock;

    public CompleteExecutionHandler(IClock clock) => _clock = clock;

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CompleteExecutionCommand)
            return;

        var aggregate = (ExecutionAggregate)await context.LoadAggregateAsync(typeof(ExecutionAggregate));
        aggregate.Complete(new Timestamp(_clock.UtcNow));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
