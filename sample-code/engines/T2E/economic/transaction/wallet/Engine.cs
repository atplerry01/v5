using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Economic.Transaction;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Transaction.Wallet;

public sealed class WalletEngine
{
    private readonly WalletPolicyAdapter _policy = new();
    private readonly IWalletDomainService _walletDomainService;

    public WalletEngine(IWalletDomainService walletDomainService)
    {
        _walletDomainService = walletDomainService;
    }

    public async Task<EngineResult> ExecuteAsync(WalletCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            CreateWalletCommand c => await CreateAsync(c, context, ct),
            FreezeWalletCommand c => await FreezeAsync(c, context, ct),
            UnfreezeWalletCommand c => await UnfreezeAsync(c, context, ct),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> CreateAsync(CreateWalletCommand command, EngineContext context, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validation = await context.ValidateAsync(command.IdentityId, ct);
        if (!validation.IsValid)
            return EngineResult.Fail(validation.Reason ?? "Validation failed", "VALIDATION_FAILED");

        var execCtx = new DomainExecutionContext
        {
            CorrelationId = context.CorrelationId,
            ActorId = context.Headers.GetValueOrDefault("x-actor-id") ?? "system",
            Action = command.GetType().Name,
            Domain = "economic.transaction",
            CommandType = context.CommandType,
            PolicyId = context.Headers.GetValueOrDefault("x-policy-id"),
            Timestamp = DateTimeOffset.UtcNow
        };

        var result = await _walletDomainService.CreateAsync(execCtx, command.WalletId, command.IdentityId, command.CurrencyCode);

        if (!result.Success)
            return EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);

        return EngineResult.Ok(new WalletDto(command.WalletId, command.IdentityId, command.CurrencyCode, "Active"));
    }

    private async Task<EngineResult> FreezeAsync(FreezeWalletCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = new DomainExecutionContext
        {
            CorrelationId = context.CorrelationId,
            ActorId = context.Headers.GetValueOrDefault("x-actor-id") ?? "system",
            Action = command.GetType().Name,
            Domain = "economic.transaction",
            CommandType = context.CommandType,
            PolicyId = context.Headers.GetValueOrDefault("x-policy-id"),
            Timestamp = DateTimeOffset.UtcNow
        };

        var result = await _walletDomainService.FreezeAsync(execCtx, command.WalletId, command.Reason);
        return result.Success
            ? EngineResult.Ok(new WalletDto(command.WalletId, "", "", "Frozen"))
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private async Task<EngineResult> UnfreezeAsync(UnfreezeWalletCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = new DomainExecutionContext
        {
            CorrelationId = context.CorrelationId,
            ActorId = context.Headers.GetValueOrDefault("x-actor-id") ?? "system",
            Action = command.GetType().Name,
            Domain = "economic.transaction",
            CommandType = context.CommandType,
            PolicyId = context.Headers.GetValueOrDefault("x-policy-id"),
            Timestamp = DateTimeOffset.UtcNow
        };

        var result = await _walletDomainService.UnfreezeAsync(execCtx, command.WalletId);
        return result.Success
            ? EngineResult.Ok(new WalletDto(command.WalletId, "", "", "Active"))
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }
}
