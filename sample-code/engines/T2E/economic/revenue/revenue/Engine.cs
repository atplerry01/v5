using Whycespace.Domain.EconomicSystem.Ledger.Obligation;
using Whycespace.Domain.EconomicSystem.Revenue.Revenue;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Shared.Contracts.Domain.Economic;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Revenue.Revenue;

public sealed class RevenueEngine : IEngine<RevenueCommand>
{
    private readonly RevenuePolicyAdapter _policy = new();
    private readonly IMoneyFactory _moneyFactory;
    private readonly IObligationStatusFactory _obligationStatusFactory;
    private readonly IRevenueAggregateFactory _revenueFactory;
    private readonly IRevenueInvariantValidator _invariantValidator;

    public RevenueEngine(
        IMoneyFactory moneyFactory,
        IObligationStatusFactory obligationStatusFactory,
        IRevenueAggregateFactory revenueFactory,
        IRevenueInvariantValidator invariantValidator)
    {
        _moneyFactory = moneyFactory ?? throw new ArgumentNullException(nameof(moneyFactory));
        _obligationStatusFactory = obligationStatusFactory ?? throw new ArgumentNullException(nameof(obligationStatusFactory));
        _revenueFactory = revenueFactory ?? throw new ArgumentNullException(nameof(revenueFactory));
        _invariantValidator = invariantValidator ?? throw new ArgumentNullException(nameof(invariantValidator));
    }

    public async Task<EngineResult> ExecuteAsync(RevenueCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            CreateRevenueCommand c => await CreateAsync(c, context),
            RecognizeRevenueCommand c => await RecognizeAsync(c, context),
            ReverseRevenueCommand c => await ReverseAsync(c, context),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> CreateAsync(CreateRevenueCommand command, EngineContext context)
    {
        var aggregate = _revenueFactory.Create(
            Guid.Parse(command.Id),
            Guid.Parse(command.SettlementId),
            command.Amount,
            command.CurrencyCode);

        if (aggregate is IEventSource eventSource)
        {
            await context.EmitEvents(eventSource);
        }

        return EngineResult.Ok(context.EmittedEvents);
    }

    private async Task<EngineResult> RecognizeAsync(RecognizeRevenueCommand command, EngineContext context)
    {
        var aggregate = await context.LoadAggregate<RevenueAggregate>(command.RevenueId);
        var money = _moneyFactory.CreateMoney(command.Amount, command.CurrencyCode);
        var obligationStatus = _obligationStatusFactory.Create(command.ObligationStatus);

        var invariantService = new RevenueInvariantService();
        aggregate.RecognizeRevenue((Money)money, (ObligationStatus)obligationStatus, invariantService);
        await context.EmitEvents(aggregate);
        return EngineResult.Ok(context.EmittedEvents);
    }

    private async Task<EngineResult> ReverseAsync(ReverseRevenueCommand command, EngineContext context)
    {
        var aggregate = await context.LoadAggregate<RevenueAggregate>(command.RevenueId);
        var money = (Money)_moneyFactory.CreateMoney(command.Amount, command.CurrencyCode);
        aggregate.ReverseRevenue(money);
        await context.EmitEvents(aggregate);
        return EngineResult.Ok(context.EmittedEvents);
    }
}
