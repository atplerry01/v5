using Whycespace.Domain.ContentSystem.Document.LifecycleChange.Version;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.LifecycleChange.Version;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.LifecycleChange.Version;

public sealed class WithdrawDocumentVersionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not WithdrawDocumentVersionCommand cmd)
            return;

        var aggregate = (DocumentVersionAggregate)await context.LoadAggregateAsync(typeof(DocumentVersionAggregate));
        aggregate.Withdraw(cmd.Reason, new Timestamp(cmd.WithdrawnAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
