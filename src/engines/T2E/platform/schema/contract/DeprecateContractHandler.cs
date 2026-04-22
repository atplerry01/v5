using Whycespace.Domain.PlatformSystem.Schema.Contract;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Schema.Contract;

namespace Whycespace.Engines.T2E.Platform.Schema.Contract;

public sealed class DeprecateContractHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DeprecateContractCommand cmd)
            return;

        var aggregate = (ContractAggregate)await context.LoadAggregateAsync(typeof(ContractAggregate));
        aggregate.Deprecate(new Timestamp(cmd.DeprecatedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
