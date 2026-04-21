using Whycespace.Domain.ContentSystem.Media.CoreObject.Subtitle;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Media.CoreObject.Subtitle;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Media.CoreObject.Subtitle;

public sealed class UpdateSubtitleHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not UpdateSubtitleCommand cmd) return;
        var aggregate = (SubtitleAggregate)await context.LoadAggregateAsync(typeof(SubtitleAggregate));
        aggregate.Update(new SubtitleOutputRef(cmd.OutputRef), new Timestamp(cmd.UpdatedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
