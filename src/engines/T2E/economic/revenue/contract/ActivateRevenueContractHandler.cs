using Whycespace.Domain.EconomicSystem.Revenue.Contract;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Revenue.Contract;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T2E.Economic.Revenue.Contract;

public sealed class ActivateRevenueContractHandler : IEngine
{
    private readonly IClock _clock;

    public ActivateRevenueContractHandler(IClock clock) => _clock = clock;

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ActivateRevenueContractCommand)
            return;

        var aggregate = (RevenueContractAggregate)await context.LoadAggregateAsync(typeof(RevenueContractAggregate));
        aggregate.Activate(new Timestamp(_clock.UtcNow));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
