using Whycespace.Domain.ContentSystem.Media.CoreObject.Transcript;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Media.CoreObject.Transcript;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Media.CoreObject.Transcript;

public sealed class CreateTranscriptHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateTranscriptCommand cmd) return Task.CompletedTask;
        MediaFileRef? sourceFileRef = cmd.SourceFileRef.HasValue ? new MediaFileRef(cmd.SourceFileRef.Value) : null;
        var aggregate = TranscriptAggregate.Create(
            new TranscriptId(cmd.TranscriptId),
            new MediaAssetRef(cmd.AssetRef),
            sourceFileRef,
            Enum.Parse<TranscriptFormat>(cmd.Format),
            new TranscriptLanguage(cmd.Language),
            new Timestamp(cmd.CreatedAt));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
