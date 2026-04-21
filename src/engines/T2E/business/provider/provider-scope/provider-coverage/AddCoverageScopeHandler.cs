using Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderCoverage;
using Whycespace.Shared.Contracts.Business.Provider.ProviderScope.ProviderCoverage;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Provider.ProviderScope.ProviderCoverage;

public sealed class AddCoverageScopeHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not AddCoverageScopeCommand cmd)
            return;

        var aggregate = (ProviderCoverageAggregate)await context.LoadAggregateAsync(typeof(ProviderCoverageAggregate));
        // ScopeKind is transported as the enum name so the wire schema stays
        // decoupled from the CoverageScopeKind CLR type. The domain value-object
        // guard will re-validate the enum value, so an unknown name fails loudly.
        var kind = Enum.Parse<CoverageScopeKind>(cmd.ScopeKind, ignoreCase: false);
        aggregate.AddScope(new CoverageScope(kind, cmd.ScopeDescriptor));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
