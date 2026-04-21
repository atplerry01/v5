using Whycespace.Domain.ContentSystem.Document.LifecycleChange.Processing;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.LifecycleChange.Processing;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.LifecycleChange.Processing;

public sealed class RequestDocumentProcessingHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RequestDocumentProcessingCommand cmd)
            return Task.CompletedTask;

        var aggregate = DocumentProcessingAggregate.Request(
            new ProcessingJobId(cmd.JobId),
            Enum.Parse<ProcessingKind>(cmd.Kind),
            new ProcessingInputRef(cmd.InputRef),
            new Timestamp(cmd.RequestedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
