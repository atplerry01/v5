using Whycespace.Domain.EconomicSystem.Transaction.Charge;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Transaction.Charge;

public sealed class ChargeEngine : IEngine<ChargeCommand>
{
    private readonly ChargePolicyAdapter _policy = new();

    public async Task<EngineResult> ExecuteAsync(ChargeCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            CreateChargeCommand c => await CreateAsync(c, context),
            ApproveChargeCommand c => await ApproveAsync(c, context),
            RejectChargeCommand c => await RejectAsync(c, context),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private static async Task<EngineResult> CreateAsync(CreateChargeCommand command, EngineContext context)
    {
        var aggregate = ChargeAggregate.Create(
            Guid.Parse(command.Id),
            Guid.Parse(command.WalletId),
            new ChargeAmount(command.Amount),
            new ChargeType(command.CurrencyCode, command.Description));
        await context.EmitEvents(aggregate);
        return EngineResult.Ok(context.EmittedEvents);
    }

    private static async Task<EngineResult> ApproveAsync(ApproveChargeCommand command, EngineContext context)
    {
        var aggregate = await context.LoadAggregate<ChargeAggregate>(command.ChargeId);
        aggregate.Approve();
        await context.EmitEvents(aggregate);
        return EngineResult.Ok(context.EmittedEvents);
    }

    private static async Task<EngineResult> RejectAsync(RejectChargeCommand command, EngineContext context)
    {
        var aggregate = await context.LoadAggregate<ChargeAggregate>(command.ChargeId);
        aggregate.Reverse(command.Reason);
        await context.EmitEvents(aggregate);
        return EngineResult.Ok(context.EmittedEvents);
    }
}
