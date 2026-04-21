using Whycespace.Domain.ContentSystem.Media.Intake.Ingest;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Media.Intake.Ingest;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Media.Intake.Ingest;

public sealed class RequestMediaIngestHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RequestMediaIngestCommand cmd) return Task.CompletedTask;
        var aggregate = MediaIngestAggregate.Request(
            new MediaIngestId(cmd.UploadId),
            new MediaIngestSourceRef(cmd.SourceRef),
            new MediaIngestInputRef(cmd.InputRef),
            new Timestamp(cmd.RequestedAt));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
