using Whycespace.Domain.ControlSystem.SystemPolicy.PolicyPackage;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyPackage;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.SystemPolicy.PolicyPackage;

public sealed class RetirePolicyPackageHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RetirePolicyPackageCommand)
            return;

        var aggregate = (PolicyPackageAggregate)await context.LoadAggregateAsync(typeof(PolicyPackageAggregate));
        aggregate.Retire();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
