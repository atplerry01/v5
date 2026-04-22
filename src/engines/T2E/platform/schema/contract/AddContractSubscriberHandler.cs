using Whycespace.Domain.PlatformSystem.Schema.Contract;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Schema.Contract;

namespace Whycespace.Engines.T2E.Platform.Schema.Contract;

public sealed class AddContractSubscriberHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not AddContractSubscriberCommand cmd)
            return;

        var aggregate = (ContractAggregate)await context.LoadAggregateAsync(typeof(ContractAggregate));

        var compatibilityMode = cmd.RequiredCompatibilityMode switch
        {
            "Backward" => ContractCompatibilityMode.Backward,
            "Forward" => ContractCompatibilityMode.Forward,
            "Full" => ContractCompatibilityMode.Full,
            _ => ContractCompatibilityMode.None
        };

        var constraint = new SubscriberConstraint(
            new DomainRoute(cmd.SubscriberClassification, cmd.SubscriberContext, cmd.SubscriberDomain),
            cmd.MinSchemaVersion,
            compatibilityMode);

        aggregate.AddSubscriber(constraint, new Timestamp(cmd.AddedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
