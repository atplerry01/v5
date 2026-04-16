using Whycespace.Domain.EconomicSystem.Ledger.Obligation;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Ledger.Obligation;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T2E.Economic.Ledger.Obligation;

public sealed class FulfilObligationHandler : IEngine
{
    private readonly IClock _clock;

    public FulfilObligationHandler(IClock clock) => _clock = clock;

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not FulfilObligationCommand cmd)
            return;

        var obligation = (ObligationAggregate)await context.LoadAggregateAsync(typeof(ObligationAggregate));
        obligation.Fulfil(cmd.SettlementId, new Timestamp(_clock.UtcNow));

        context.EmitEvents(obligation.DomainEvents);
    }
}
