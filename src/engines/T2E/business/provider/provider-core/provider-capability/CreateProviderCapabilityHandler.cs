using Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderCapability;
using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Shared.Contracts.Business.Provider.ProviderCore.ProviderCapability;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Provider.ProviderCore.ProviderCapability;

public sealed class CreateProviderCapabilityHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateProviderCapabilityCommand cmd)
            return Task.CompletedTask;

        var aggregate = ProviderCapabilityAggregate.Create(
            new ProviderCapabilityId(cmd.ProviderCapabilityId),
            new ClusterProviderRef(cmd.ProviderId),
            new CapabilityCode(cmd.Code),
            new CapabilityName(cmd.Name));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
