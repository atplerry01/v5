using Whycespace.Domain.EconomicSystem.Capital.Vault;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Capital.Vault;

public sealed class VaultEngine : IEngine<VaultCommand>
{
    private readonly VaultPolicyAdapter _policy = new();

    public async Task<EngineResult> ExecuteAsync(VaultCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            CreateVaultCommand c => await CreateAsync(c, context),
            DepositFundsCommand c => await DepositAsync(c, context),
            WithdrawFundsCommand c => await WithdrawAsync(c, context),
            LockFundsCommand c => await LockAsync(c, context),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private static async Task<EngineResult> CreateAsync(CreateVaultCommand command, EngineContext context)
    {
        var aggregate = await context.LoadAggregate<VaultAggregate>(command.Id);
        aggregate.Create(Guid.Parse(command.Id));
        await context.EmitEvents(aggregate);
        return EngineResult.Ok(context.EmittedEvents);
    }

    private static async Task<EngineResult> DepositAsync(DepositFundsCommand command, EngineContext context)
    {
        var aggregate = await context.LoadAggregate<VaultAggregate>(command.VaultId);
        aggregate.Credit(Guid.Parse(command.VaultId), command.Amount, command.CurrencyCode);
        await context.EmitEvents(aggregate);
        return EngineResult.Ok(context.EmittedEvents);
    }

    private static async Task<EngineResult> WithdrawAsync(WithdrawFundsCommand command, EngineContext context)
    {
        var aggregate = await context.LoadAggregate<VaultAggregate>(command.VaultId);
        aggregate.Debit(Guid.Parse(command.VaultId), command.Amount, command.CurrencyCode);
        await context.EmitEvents(aggregate);
        return EngineResult.Ok(context.EmittedEvents);
    }

    private static async Task<EngineResult> LockAsync(LockFundsCommand command, EngineContext context)
    {
        var aggregate = await context.LoadAggregate<VaultAggregate>(command.VaultId);
        aggregate.Lock(Guid.Parse(command.VaultId), command.Amount, command.Reason);
        await context.EmitEvents(aggregate);
        return EngineResult.Ok(context.EmittedEvents);
    }
}
