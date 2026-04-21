using Whycespace.Domain.ContentSystem.Media.TechnicalProcessing.Processing;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Media.TechnicalProcessing.Processing;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Media.TechnicalProcessing.Processing;

public sealed class RequestMediaProcessingHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RequestMediaProcessingCommand cmd) return Task.CompletedTask;
        var aggregate = MediaProcessingAggregate.Request(
            new MediaProcessingJobId(cmd.JobId),
            Enum.Parse<MediaProcessingKind>(cmd.Kind),
            new MediaProcessingInputRef(cmd.InputRef),
            new Timestamp(cmd.RequestedAt));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
