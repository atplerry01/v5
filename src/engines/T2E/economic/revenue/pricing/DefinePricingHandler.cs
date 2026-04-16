using Whycespace.Domain.EconomicSystem.Revenue.Pricing;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Revenue.Pricing;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T2E.Economic.Revenue.Pricing;

public sealed class DefinePricingHandler : IEngine
{
    private readonly IClock _clock;

    public DefinePricingHandler(IClock clock) => _clock = clock;

    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DefinePricingCommand cmd)
            return Task.CompletedTask;

        if (!Enum.TryParse<PricingModel>(cmd.Model, ignoreCase: true, out var model))
            throw new ArgumentException($"Unknown PricingModel: {cmd.Model}", nameof(cmd.Model));

        var aggregate = PricingAggregate.DefinePrice(
            new PricingId(cmd.PricingId),
            cmd.ContractId,
            model,
            new Amount(cmd.Price),
            new Currency(cmd.Currency),
            new Timestamp(_clock.UtcNow));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
