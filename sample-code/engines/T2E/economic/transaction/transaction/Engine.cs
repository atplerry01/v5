using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Economic.Transaction;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Transaction.Transaction;

public sealed class TransactionEngine : IEngine<TransactionCommand>
{
    private readonly TransactionPolicyAdapter _policy = new();
    private readonly ITransactionDomainService _transactionDomainService;

    public TransactionEngine(ITransactionDomainService transactionDomainService)
    {
        _transactionDomainService = transactionDomainService;
    }

    public async Task<EngineResult> ExecuteAsync(TransactionCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            InitiateTransactionCommand c => await InitiateAsync(c, context, ct),
            CompleteTransactionCommand c => await CompleteAsync(c, context, ct),
            RejectTransactionCommand c => await RejectAsync(c, context, ct),
            SettleTransactionCommand c => await SettleAsync(c, context, ct),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private static DomainExecutionContext BuildExecCtx(object command, EngineContext context)
    {
        return new DomainExecutionContext
        {
            CorrelationId = context.CorrelationId,
            ActorId = context.Headers.GetValueOrDefault("x-actor-id") ?? "system",
            Action = command.GetType().Name,
            Domain = "economic.transaction",
            CommandType = context.CommandType,
            PolicyId = context.Headers.GetValueOrDefault("x-policy-id"),
            Timestamp = DateTimeOffset.UtcNow
        };
    }

    private async Task<EngineResult> InitiateAsync(InitiateTransactionCommand command, EngineContext context, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validation = await context.ValidateAsync(command.TransactionId, ct);
        if (!validation.IsValid)
            return EngineResult.Fail(validation.Reason ?? "Validation failed", "VALIDATION_FAILED");

        if (command.SourceWalletId == command.DestinationWalletId)
            return EngineResult.Fail("Source and destination wallets must be different", "SAME_WALLET");

        if (!command.IsSourceWalletActive)
            return EngineResult.Fail("Source wallet is not active", "SOURCE_WALLET_INACTIVE");
        if (command.IsSourceWalletFrozen)
            return EngineResult.Fail("Source wallet is frozen", "SOURCE_WALLET_FROZEN");
        if (!command.IsDestinationWalletActive)
            return EngineResult.Fail("Destination wallet is not active", "DESTINATION_WALLET_INACTIVE");
        if (command.IsDestinationWalletFrozen)
            return EngineResult.Fail("Destination wallet is frozen", "DESTINATION_WALLET_FROZEN");

        if (!string.Equals(command.EnforcementDecision, "Allow", StringComparison.OrdinalIgnoreCase))
            return EngineResult.Fail(
                $"Transaction blocked by enforcement: {command.EnforcementReasonCode ?? command.EnforcementDecision}",
                "ENFORCEMENT_BLOCKED");

        if (!command.IsWithinLimits)
            return EngineResult.Fail("Transaction exceeds limits", "LIMIT_EXCEEDED");

        var execCtx = BuildExecCtx(command, context);

        var initiateResult = await _transactionDomainService.InitiateAsync(
            execCtx,
            command.TransactionId, command.SourceWalletId, command.DestinationWalletId,
            command.Amount, command.CurrencyCode);

        if (!initiateResult.Success)
            return EngineResult.Fail(initiateResult.ErrorMessage!, initiateResult.ErrorCode);

        var approveResult = await _transactionDomainService.ApproveAsync(execCtx, command.TransactionId);
        if (!approveResult.Success)
            return EngineResult.Fail(approveResult.ErrorMessage!, approveResult.ErrorCode);

        return EngineResult.Ok(new TransactionDto(
            command.TransactionId, command.SourceWalletId, command.DestinationWalletId,
            command.Amount, command.CurrencyCode, "Approved"));
    }

    private async Task<EngineResult> CompleteAsync(CompleteTransactionCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildExecCtx(command, context);
        var result = await _transactionDomainService.CompleteAsync(execCtx, command.TransactionId);
        return result.Success
            ? EngineResult.Ok(new { command.TransactionId, Status = "Completed" })
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private async Task<EngineResult> RejectAsync(RejectTransactionCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildExecCtx(command, context);
        var result = await _transactionDomainService.RejectAsync(execCtx, command.TransactionId, command.Reason);
        return result.Success
            ? EngineResult.Ok(new { command.TransactionId, Status = "Rejected", command.Reason })
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }

    private async Task<EngineResult> SettleAsync(SettleTransactionCommand command, EngineContext context, CancellationToken ct)
    {
        var execCtx = BuildExecCtx(command, context);
        var result = await _transactionDomainService.SettleAsync(execCtx, command.TransactionId);
        return result.Success
            ? EngineResult.Ok(new { command.TransactionId, Status = "Settled" })
            : EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);
    }
}
