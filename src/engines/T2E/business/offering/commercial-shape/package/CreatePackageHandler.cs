using Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Package;
using Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Package;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Offering.CommercialShape.Package;

public sealed class CreatePackageHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreatePackageCommand cmd)
            return Task.CompletedTask;

        var aggregate = PackageAggregate.Create(
            new PackageId(cmd.PackageId),
            new PackageCode(cmd.Code),
            new PackageName(cmd.Name));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
