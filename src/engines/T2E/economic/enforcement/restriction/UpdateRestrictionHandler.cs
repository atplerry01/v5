using Whycespace.Domain.EconomicSystem.Enforcement.Restriction;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Enforcement.Restriction;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Enforcement.Restriction;

public sealed class UpdateRestrictionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not UpdateRestrictionCommand cmd)
            return;

        if (!Enum.TryParse<RestrictionScope>(cmd.NewScope, ignoreCase: true, out var scope))
            throw new InvalidOperationException($"Unknown restriction scope: '{cmd.NewScope}'.");

        var aggregate = (RestrictionAggregate)await context.LoadAggregateAsync(typeof(RestrictionAggregate));
        aggregate.Update(scope, new Reason(cmd.NewReason), new Timestamp(cmd.UpdatedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
