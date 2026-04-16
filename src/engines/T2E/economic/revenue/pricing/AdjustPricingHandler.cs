using Whycespace.Domain.EconomicSystem.Revenue.Pricing;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Revenue.Pricing;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T2E.Economic.Revenue.Pricing;

public sealed class AdjustPricingHandler : IEngine
{
    private readonly IClock _clock;

    public AdjustPricingHandler(IClock clock) => _clock = clock;

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not AdjustPricingCommand cmd)
            return;

        var aggregate = (PricingAggregate)await context.LoadAggregateAsync(typeof(PricingAggregate));
        aggregate.AdjustPrice(new Amount(cmd.NewPrice), cmd.Reason, new Timestamp(_clock.UtcNow));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
