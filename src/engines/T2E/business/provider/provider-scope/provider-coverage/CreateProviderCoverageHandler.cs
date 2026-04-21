using Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderCoverage;
using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Shared.Contracts.Business.Provider.ProviderScope.ProviderCoverage;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Provider.ProviderScope.ProviderCoverage;

public sealed class CreateProviderCoverageHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateProviderCoverageCommand cmd)
            return Task.CompletedTask;

        var aggregate = ProviderCoverageAggregate.Create(
            new ProviderCoverageId(cmd.ProviderCoverageId),
            new ClusterProviderRef(cmd.ProviderId));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
