using Whycespace.Domain.ContentSystem.Streaming.StreamCore.Channel;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Channel;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.StreamCore.Channel;

public sealed class CreateChannelHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateChannelCommand cmd) return Task.CompletedTask;
        var aggregate = ChannelAggregate.Create(
            new ChannelId(cmd.ChannelId),
            new StreamRef(cmd.StreamId),
            new ChannelName(cmd.Name),
            Enum.Parse<ChannelMode>(cmd.Mode),
            new Timestamp(cmd.CreatedAt));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
