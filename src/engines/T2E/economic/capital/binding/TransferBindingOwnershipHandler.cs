using Whycespace.Domain.EconomicSystem.Capital.Binding;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Capital.Binding;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Capital.Binding;

public sealed class TransferBindingOwnershipHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not TransferBindingOwnershipCommand cmd)
            return;

        var aggregate = (BindingAggregate)await context.LoadAggregateAsync(typeof(BindingAggregate));
        aggregate.TransferOwnership(
            cmd.NewOwnerId,
            (OwnershipType)cmd.NewOwnershipType,
            new Timestamp(cmd.TransferredAt));

        context.EmitEvents(aggregate.DomainEvents);
    }
}
