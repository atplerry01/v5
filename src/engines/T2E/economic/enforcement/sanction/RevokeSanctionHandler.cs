using Whycespace.Domain.EconomicSystem.Enforcement.Sanction;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Enforcement.Sanction;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Enforcement.Sanction;

public sealed class RevokeSanctionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RevokeSanctionCommand cmd)
            return;

        var aggregate = (SanctionAggregate)await context.LoadAggregateAsync(typeof(SanctionAggregate));
        aggregate.Revoke(new Reason(cmd.RevocationReason), new Timestamp(cmd.RevokedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
