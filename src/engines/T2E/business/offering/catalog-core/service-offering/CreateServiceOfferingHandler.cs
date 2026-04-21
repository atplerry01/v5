using Whycespace.Domain.BusinessSystem.Offering.CatalogCore.ServiceOffering;
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.ServiceOffering;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Offering.CatalogCore.ServiceOffering;

public sealed class CreateServiceOfferingHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateServiceOfferingCommand cmd)
            return Task.CompletedTask;

        OfferingPackageRef? package = cmd.PackageId is { } pid
            ? new OfferingPackageRef(pid)
            : null;

        var aggregate = ServiceOfferingAggregate.Create(
            new ServiceOfferingId(cmd.ServiceOfferingId),
            new ServiceOfferingName(cmd.Name),
            new ServiceDefinitionRef(cmd.ServiceDefinitionId),
            package);
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
