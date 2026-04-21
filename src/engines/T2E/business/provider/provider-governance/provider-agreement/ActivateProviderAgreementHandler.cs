using Whycespace.Domain.BusinessSystem.Provider.ProviderGovernance.ProviderAgreement;
using Whycespace.Domain.BusinessSystem.Shared.Time;
using Whycespace.Shared.Contracts.Business.Provider.ProviderGovernance.ProviderAgreement;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Provider.ProviderGovernance.ProviderAgreement;

public sealed class ActivateProviderAgreementHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ActivateProviderAgreementCommand cmd)
            return;

        var aggregate = (ProviderAgreementAggregate)await context.LoadAggregateAsync(typeof(ProviderAgreementAggregate));
        aggregate.Activate(new TimeWindow(cmd.EffectiveStartsAt, cmd.EffectiveEndsAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
