using Whycespace.Domain.EconomicSystem.Routing.Path;
using Whycespace.Shared.Contracts.Economic.Routing.Path;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Routing.Path;

public sealed class DefineRoutingPathHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DefineRoutingPathCommand cmd)
            return Task.CompletedTask;

        if (!Enum.TryParse<PathType>(cmd.PathType, ignoreCase: true, out var pathType))
            throw new InvalidOperationException($"Unknown routing path type: '{cmd.PathType}'.");

        var aggregate = RoutingPathAggregate.DefinePath(
            new PathId(cmd.PathId),
            pathType,
            cmd.Conditions,
            cmd.Priority);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
