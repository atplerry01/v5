using Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceConstraint;
using Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.ServiceConstraint;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Service.ServiceConstraint.ServiceConstraint;

public sealed class ActivateServiceConstraintHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ActivateServiceConstraintCommand)
            return;

        var aggregate = (ServiceConstraintAggregate)await context.LoadAggregateAsync(typeof(ServiceConstraintAggregate));
        aggregate.Activate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
