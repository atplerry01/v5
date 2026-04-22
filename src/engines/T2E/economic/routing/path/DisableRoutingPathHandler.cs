using Whycespace.Domain.OperationalSystem.Routing.Path;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Routing.Path;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T2E.Economic.Routing.Path;

public sealed class DisableRoutingPathHandler : IEngine
{
    private readonly IClock _clock;

    public DisableRoutingPathHandler(IClock clock) => _clock = clock;

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DisableRoutingPathCommand)
            return;

        var aggregate = (RoutingPathAggregate)await context.LoadAggregateAsync(typeof(RoutingPathAggregate));
        aggregate.Disable(new Timestamp(_clock.UtcNow));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
