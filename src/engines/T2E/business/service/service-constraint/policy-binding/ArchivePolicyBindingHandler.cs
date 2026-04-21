using Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.PolicyBinding;
using Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.PolicyBinding;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Service.ServiceConstraint.PolicyBinding;

public sealed class ArchivePolicyBindingHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ArchivePolicyBindingCommand)
            return;

        var aggregate = (PolicyBindingAggregate)await context.LoadAggregateAsync(typeof(PolicyBindingAggregate));
        aggregate.Archive();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
