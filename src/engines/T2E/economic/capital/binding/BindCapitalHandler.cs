using Whycespace.Domain.EconomicSystem.Capital.Binding;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Capital.Binding;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Capital.Binding;

public sealed class BindCapitalHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not BindCapitalCommand cmd)
            return Task.CompletedTask;

        var aggregate = BindingAggregate.Bind(
            new BindingId(cmd.BindingId),
            cmd.AccountId,
            cmd.OwnerId,
            (OwnershipType)cmd.OwnershipType,
            new Timestamp(cmd.BoundAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
