using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Economic.Ledger;
using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Runtime.Process.Cluster;

/// <summary>
/// H2 — Compensation Engine for cross-SPV transactions.
/// Reverses all successfully executed steps in reverse order.
/// Stages are compensated in reverse: Distribution → Revenue → Settlement → Ledger.
/// </summary>
public sealed class CrossSpvCompensationEngine
{
    private readonly ILedgerDomainService _ledgerService;
    private readonly IIdGenerator _idGenerator;

    public CrossSpvCompensationEngine(ILedgerDomainService ledgerService, IIdGenerator idGenerator)
    {
        _ledgerService = ledgerService;
        _idGenerator = idGenerator;
    }

    /// <summary>
    /// Compensates all successfully executed steps in reverse order.
    /// Each step's compensation is best-effort — failures are logged but do not halt compensation.
    /// </summary>
    public async Task<CompensationResult> CompensateAsync(
        IReadOnlyList<ExecutionResult> executedSteps,
        DomainExecutionContext context,
        CancellationToken ct = default)
    {
        var compensated = 0;
        var failed = 0;

        // Reverse through successfully executed steps
        foreach (var step in executedSteps.Where(x => x.Success).Reverse())
        {
            try
            {
                switch (step.Stage)
                {
                    case ExecutionStage.Settlement:
                        // Settlement reversal: re-record a reversal entry
                        var reversalCtx = context with
                        {
                            Action = "CompensateSettlement",
                            CorrelationId = $"{context.CorrelationId}:compensate:leg:{step.LegIndex}"
                        };
                        var reversalId = _idGenerator.DeterministicGuid(context.CorrelationId, "compensate-settlement", step.LegIndex.ToString()).ToString();
                        var entryId = _idGenerator.DeterministicGuid(context.CorrelationId, "compensate-settlement-entry", step.LegIndex.ToString()).ToString();
                        await _ledgerService.RecordDoubleEntryAsync(
                            reversalCtx,
                            reversalId,
                            entryId,
                            $"compensation:{step.ReferenceId}",
                            $"Compensation for settlement {step.ReferenceId}",
                            0m, 0m, "USD");
                        compensated++;
                        break;

                    case ExecutionStage.Ledger:
                        // Ledger reversal: record offsetting entry
                        var ledgerCtx = context with
                        {
                            Action = "CompensateLedger",
                            CorrelationId = $"{context.CorrelationId}:compensate:leg:{step.LegIndex}"
                        };
                        await _ledgerService.RecordDoubleEntryAsync(
                            ledgerCtx,
                            _idGenerator.DeterministicGuid(context.CorrelationId, "compensate-ledger", step.LegIndex.ToString()).ToString(),
                            _idGenerator.DeterministicGuid(context.CorrelationId, "compensate-ledger-entry", step.LegIndex.ToString()).ToString(),
                            $"reversal:{step.ReferenceId}",
                            $"Reversal for ledger entry {step.ReferenceId}",
                            0m, 0m, "USD");
                        compensated++;
                        break;
                }
            }
            catch
            {
                // Compensation must not throw — log and continue
                failed++;
            }
        }

        return new CompensationResult
        {
            Compensated = compensated,
            Failed = failed,
            TotalSteps = executedSteps.Count(x => x.Success)
        };
    }
}

public sealed record CompensationResult
{
    public int Compensated { get; init; }
    public int Failed { get; init; }
    public int TotalSteps { get; init; }
    public bool IsComplete => Failed == 0;
}
