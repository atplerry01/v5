using Whycespace.Domain.ContentSystem.Media.CoreObject.Subtitle;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Media.CoreObject.Subtitle;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Media.CoreObject.Subtitle;

public sealed class CreateSubtitleHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateSubtitleCommand cmd) return Task.CompletedTask;
        MediaFileRef? sourceFileRef = cmd.SourceFileRef.HasValue ? new MediaFileRef(cmd.SourceFileRef.Value) : null;
        SubtitleWindow? window = cmd.WindowStartMs.HasValue && cmd.WindowEndMs.HasValue
            ? new SubtitleWindow(cmd.WindowStartMs.Value, cmd.WindowEndMs.Value)
            : null;
        var aggregate = SubtitleAggregate.Create(
            new SubtitleId(cmd.SubtitleId),
            new MediaAssetRef(cmd.AssetRef),
            sourceFileRef,
            Enum.Parse<SubtitleFormat>(cmd.Format),
            new SubtitleLanguage(cmd.Language),
            window,
            new Timestamp(cmd.CreatedAt));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
