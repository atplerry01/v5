using Whycespace.Domain.ContentSystem.Media.LifecycleChange.Version;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Media.LifecycleChange.Version;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Media.LifecycleChange.Version;

public sealed class ActivateMediaVersionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ActivateMediaVersionCommand cmd) return;
        var aggregate = (MediaVersionAggregate)await context.LoadAggregateAsync(typeof(MediaVersionAggregate));
        aggregate.Activate(new Timestamp(cmd.ActivatedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
