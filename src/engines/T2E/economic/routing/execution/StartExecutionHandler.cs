using Whycespace.Domain.EconomicSystem.Routing.Execution;
using Whycespace.Domain.EconomicSystem.Routing.Path;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Routing.Execution;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T2E.Economic.Routing.Execution;

public sealed class StartExecutionHandler : IEngine
{
    private readonly IClock _clock;

    public StartExecutionHandler(IClock clock) => _clock = clock;

    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not StartExecutionCommand cmd)
            return Task.CompletedTask;

        var aggregate = ExecutionAggregate.Start(
            new ExecutionId(cmd.ExecutionId),
            new PathId(cmd.PathId),
            new Timestamp(_clock.UtcNow));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
