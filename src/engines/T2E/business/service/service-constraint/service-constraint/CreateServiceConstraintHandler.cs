using Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceConstraint;
using Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.ServiceConstraint;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Service.ServiceConstraint.ServiceConstraint;

public sealed class CreateServiceConstraintHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateServiceConstraintCommand cmd)
            return Task.CompletedTask;

        var aggregate = ServiceConstraintAggregate.Create(
            new ServiceConstraintId(cmd.ServiceConstraintId),
            new ServiceDefinitionRef(cmd.ServiceDefinitionId),
            (ConstraintKind)cmd.Kind,
            new ConstraintDescriptor(cmd.Descriptor));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
