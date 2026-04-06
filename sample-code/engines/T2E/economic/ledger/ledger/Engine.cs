using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Economic.Ledger;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Ledger.Ledger;

public sealed class LedgerEngine : IEngine<LedgerCommand>
{
    private readonly LedgerPolicyAdapter _policy = new();
    private readonly ILedgerDomainService _ledgerDomainService;

    public LedgerEngine(ILedgerDomainService ledgerDomainService)
    {
        _ledgerDomainService = ledgerDomainService;
    }

    public async Task<EngineResult> ExecuteAsync(LedgerCommand command, EngineContext context, CancellationToken ct)
    {
        await _policy.EnforceAsync(command);

        return command switch
        {
            RecordLedgerEntryCommand c => await RecordAsync(c, context, ct),
            _ => throw new NotSupportedException($"Unknown command: {command.GetType().Name}")
        };
    }

    private async Task<EngineResult> RecordAsync(RecordLedgerEntryCommand command, EngineContext context, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var execCtx = new DomainExecutionContext
        {
            CorrelationId = context.CorrelationId,
            ActorId = context.Headers.GetValueOrDefault("x-actor-id") ?? "system",
            Action = command.GetType().Name,
            Domain = "economic.ledger",
            CommandType = context.CommandType,
            PolicyId = context.Headers.GetValueOrDefault("x-policy-id"),
            Timestamp = DateTimeOffset.UtcNow
        };

        DomainOperationResult result;
        try
        {
            result = await _ledgerDomainService.RecordDoubleEntryAsync(
                execCtx,
                command.LedgerId, command.EntryId, command.AccountCode, command.AccountName,
                command.DebitAmount, command.CreditAmount, command.CurrencyCode);
        }
        catch (Exception ex)
        {
            return EngineResult.Fail(ex.Message, "LEDGER_EXECUTION_FAILED");
        }

        if (!result.Success)
            return EngineResult.Fail(result.ErrorMessage!, result.ErrorCode);

        return EngineResult.Ok(new LedgerEntryDto(
            command.LedgerId, command.EntryId, command.AccountCode,
            command.DebitAmount, command.CreditAmount, command.CurrencyCode));
    }
}
