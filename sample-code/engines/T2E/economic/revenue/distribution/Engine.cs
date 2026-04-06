using Whycespace.Shared.Contracts.Domain.Economic;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Revenue.Distribution;

public sealed class DistributionEngine : IEngine<DistributionCommand>
{
    private readonly DistributionPolicyAdapter _policy = new();
    private readonly IDistributionAggregateFactory _distributionFactory;

    public DistributionEngine(IDistributionAggregateFactory distributionFactory)
    {
        _distributionFactory = distributionFactory ?? throw new ArgumentNullException(nameof(distributionFactory));
    }

    public async Task<EngineResult> ExecuteAsync(DistributionCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            CreateDistributionCommand c => await CreateAsync(c, context),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> CreateAsync(CreateDistributionCommand command, EngineContext context)
    {
        var aggregate = _distributionFactory.Create(
            Guid.Parse(command.Id),
            command.RevenueId,
            command.Amount,
            command.CurrencyCode);

        if (aggregate is IEventSource eventSource)
        {
            await context.EmitEvents(eventSource);
        }

        return EngineResult.Ok(context.EmittedEvents);
    }
}
