using Whycespace.Domain.ContentSystem.Streaming.StreamCore.Stream;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Stream;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.StreamCore.Stream;

public sealed class CreateStreamHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateStreamCommand cmd) return Task.CompletedTask;
        var aggregate = StreamAggregate.Create(
            new StreamId(cmd.StreamId),
            Enum.Parse<StreamMode>(cmd.Mode),
            Enum.Parse<StreamType>(cmd.Type),
            new Timestamp(cmd.CreatedAt));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
