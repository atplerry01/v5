using Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderCoverage;
using Whycespace.Shared.Contracts.Business.Provider.ProviderScope.ProviderCoverage;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Provider.ProviderScope.ProviderCoverage;

public sealed class ArchiveProviderCoverageHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ArchiveProviderCoverageCommand)
            return;

        var aggregate = (ProviderCoverageAggregate)await context.LoadAggregateAsync(typeof(ProviderCoverageAggregate));
        aggregate.Archive();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
