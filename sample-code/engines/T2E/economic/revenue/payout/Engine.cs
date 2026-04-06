using Whycespace.Domain.EconomicSystem.Revenue.Payout;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Revenue.Payout;

public sealed class PayoutEngine : IEngine<PayoutCommand>
{
    private readonly PayoutPolicyAdapter _policy = new();

    public async Task<EngineResult> ExecuteAsync(PayoutCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            InitiatePayoutCommand c => await InitiateAsync(c, context),
            CompletePayoutCommand c => await CompleteAsync(c, context),
            CancelPayoutCommand c => await CancelAsync(c, context),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private static async Task<EngineResult> InitiateAsync(InitiatePayoutCommand command, EngineContext context)
    {
        var aggregate = PayoutAggregate.Schedule(
            Guid.Parse(command.Id),
            new PayoutRecipient(Guid.Parse(command.RecipientId), "default"),
            new PayoutAmount(command.Amount),
            command.CurrencyCode,
            DateTimeOffset.UtcNow);
        await context.EmitEvents(aggregate);
        return EngineResult.Ok(context.EmittedEvents);
    }

    private static async Task<EngineResult> CompleteAsync(CompletePayoutCommand command, EngineContext context)
    {
        var aggregate = await context.LoadAggregate<PayoutAggregate>(command.PayoutId);
        aggregate.Complete();
        await context.EmitEvents(aggregate);
        return EngineResult.Ok(context.EmittedEvents);
    }

    private static async Task<EngineResult> CancelAsync(CancelPayoutCommand command, EngineContext context)
    {
        var aggregate = await context.LoadAggregate<PayoutAggregate>(command.PayoutId);
        aggregate.Fail(command.Reason);
        await context.EmitEvents(aggregate);
        return EngineResult.Ok(context.EmittedEvents);
    }
}
