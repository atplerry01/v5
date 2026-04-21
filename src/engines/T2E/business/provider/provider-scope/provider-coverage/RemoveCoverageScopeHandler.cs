using Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderCoverage;
using Whycespace.Shared.Contracts.Business.Provider.ProviderScope.ProviderCoverage;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Provider.ProviderScope.ProviderCoverage;

public sealed class RemoveCoverageScopeHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RemoveCoverageScopeCommand cmd)
            return;

        var aggregate = (ProviderCoverageAggregate)await context.LoadAggregateAsync(typeof(ProviderCoverageAggregate));
        var kind = Enum.Parse<CoverageScopeKind>(cmd.ScopeKind, ignoreCase: false);
        aggregate.RemoveScope(new CoverageScope(kind, cmd.ScopeDescriptor));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
