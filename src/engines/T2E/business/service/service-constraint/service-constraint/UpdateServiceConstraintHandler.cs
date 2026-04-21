using Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceConstraint;
using Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.ServiceConstraint;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Service.ServiceConstraint.ServiceConstraint;

public sealed class UpdateServiceConstraintHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not UpdateServiceConstraintCommand cmd)
            return;

        var aggregate = (ServiceConstraintAggregate)await context.LoadAggregateAsync(typeof(ServiceConstraintAggregate));
        aggregate.Update((ConstraintKind)cmd.Kind, new ConstraintDescriptor(cmd.Descriptor));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
