using Whycespace.Domain.EconomicSystem.Revenue.Pricing;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Revenue.Pricing;

public sealed class PricingEngine : IEngine<PricingCommand>
{
    private readonly PricingPolicyAdapter _policy = new();

    public async Task<EngineResult> ExecuteAsync(PricingCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            CalculatePriceCommand c => await CalculateAsync(c, context),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private static async Task<EngineResult> CalculateAsync(CalculatePriceCommand command, EngineContext context)
    {
        var aggregate = PricingAggregate.Calculate(Guid.Parse(command.Id), command.Price, command.CurrencyCode);
        await context.EmitEvents(aggregate);
        return EngineResult.Ok(context.EmittedEvents);
    }
}
