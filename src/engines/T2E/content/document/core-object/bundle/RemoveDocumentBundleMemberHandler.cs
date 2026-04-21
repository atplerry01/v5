using Whycespace.Domain.ContentSystem.Document.CoreObject.Bundle;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.CoreObject.Bundle;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.CoreObject.Bundle;

public sealed class RemoveDocumentBundleMemberHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RemoveDocumentBundleMemberCommand cmd)
            return;

        var aggregate = (DocumentBundleAggregate)await context.LoadAggregateAsync(typeof(DocumentBundleAggregate));
        aggregate.RemoveMember(
            new BundleMemberRef(cmd.MemberId),
            new Timestamp(cmd.RemovedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
