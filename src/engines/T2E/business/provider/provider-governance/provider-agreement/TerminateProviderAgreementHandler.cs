using Whycespace.Domain.BusinessSystem.Provider.ProviderGovernance.ProviderAgreement;
using Whycespace.Shared.Contracts.Business.Provider.ProviderGovernance.ProviderAgreement;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Provider.ProviderGovernance.ProviderAgreement;

public sealed class TerminateProviderAgreementHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not TerminateProviderAgreementCommand cmd)
            return;

        var aggregate = (ProviderAgreementAggregate)await context.LoadAggregateAsync(typeof(ProviderAgreementAggregate));
        aggregate.Terminate(cmd.TerminatedAt);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
