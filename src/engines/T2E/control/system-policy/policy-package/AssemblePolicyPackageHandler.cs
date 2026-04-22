using Whycespace.Domain.ControlSystem.SystemPolicy.PolicyPackage;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyPackage;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.SystemPolicy.PolicyPackage;

public sealed class AssemblePolicyPackageHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not AssemblePolicyPackageCommand cmd)
            return Task.CompletedTask;

        var aggregate = PolicyPackageAggregate.Assemble(
            new PolicyPackageId(cmd.PackageId.ToString("N").PadRight(64, '0')),
            cmd.Name,
            new PackageVersion(cmd.VersionMajor, cmd.VersionMinor),
            cmd.PolicyDefinitionIds);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
