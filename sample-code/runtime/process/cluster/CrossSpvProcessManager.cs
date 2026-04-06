using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Economic.Ledger;
using Whycespace.Shared.Contracts.Domain.Structural;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Domain.StructuralSystem.Cluster.Spv.Transaction;

namespace Whycespace.Runtime.Process.Cluster;

/// <summary>
/// E18.5 H1 — Atomic Cross-SPV Process Manager (Saga Coordinator)
///
/// 3-phase execution model:
///   PHASE 1 — PREPARE: Create aggregate + prepare (policy-gated, chain-anchored)
///   PHASE 2 — EXECUTE: For each leg, create ledger entries + settle (tracked per-step)
///   PHASE 3 — COMMIT or COMPENSATE: All-or-nothing finalization
///
/// Invariant: all legs must succeed or all must roll back.
/// No partial economic state is permitted.
/// </summary>
public sealed class CrossSpvProcessManager
{
    private readonly ICrossSpvDomainService _crossSpvService;
    private readonly ILedgerDomainService _ledgerService;
    private readonly CrossSpvCompensationEngine _compensationEngine;
    private readonly IIdGenerator _idGenerator;

    public CrossSpvProcessManager(
        ICrossSpvDomainService crossSpvService,
        ILedgerDomainService ledgerService,
        CrossSpvCompensationEngine compensationEngine,
        IIdGenerator idGenerator)
    {
        _crossSpvService = crossSpvService;
        _ledgerService = ledgerService;
        _compensationEngine = compensationEngine;
        _idGenerator = idGenerator;
    }

    /// <summary>
    /// Executes the full cross-SPV flow with atomic guarantees.
    /// </summary>
    public async Task<CrossSpvProcessResult> ExecuteAsync(
        string transactionId,
        string rootSpvId,
        IReadOnlyList<SpvLegDto> legs,
        DomainExecutionContext context,
        CancellationToken ct = default)
    {
        var id = _idGenerator.DeterministicGuid(transactionId, context.CorrelationId, "crossspv").ToString();
        var correlationId = context.CorrelationId;

        // ── PHASE 0: CREATE ─────────────────────────────────────
        var createResult = await _crossSpvService.CreateCrossSpvTransactionAsync(
            context, id, rootSpvId, legs);

        if (!createResult.Success)
            return CrossSpvProcessResult.Fail(createResult.ErrorMessage ?? "Create failed");

        // ── PHASE 1: PREPARE (policy evaluation + chain anchoring) ──
        await _crossSpvService.SetExecutionStateAsync(
            context with { Action = "SetExecutionState" },
            id, CrossSpvExecutionState.Preparing.Value);

        var prepareCtx = context with { Action = "PrepareCrossSpvTransaction" };
        var prepareResult = await _crossSpvService.PrepareCrossSpvTransactionAsync(
            prepareCtx, id, transactionId);

        if (!prepareResult.Success)
        {
            await FailTransaction(context, id, transactionId, prepareResult.ErrorMessage ?? "Prepare failed");
            return CrossSpvProcessResult.Fail(prepareResult.ErrorMessage ?? "Prepare failed");
        }

        // ── PHASE 2: EXECUTE (all legs, tracked per-step) ───────
        await _crossSpvService.SetExecutionStateAsync(
            context with { Action = "SetExecutionState" },
            id, CrossSpvExecutionState.Executing.Value);

        var executionResults = new List<ExecutionResult>();

        try
        {
            for (var i = 0; i < legs.Count; i++)
            {
                var leg = legs[i];
                var legCorrelation = $"{correlationId}:leg:{i}";

                // Step 1: Ledger double-entry
                var ledgerId = _idGenerator.DeterministicGuid(correlationId, "ledger", i.ToString());
                var ledgerCtx = context with
                {
                    Action = "RecordDoubleEntry",
                    CorrelationId = legCorrelation
                };

                var ledgerResult = await _ledgerService.RecordDoubleEntryAsync(
                    ledgerCtx,
                    ledgerId.ToString(),
                    _idGenerator.DeterministicGuid(correlationId, "ledger-entry", i.ToString()).ToString(),
                    $"spv:{leg.FromSpvId}",
                    $"SPV {leg.FromSpvId} → SPV {leg.ToSpvId}",
                    leg.Amount,
                    leg.Amount,
                    leg.CurrencyCode);

                if (!ledgerResult.Success)
                {
                    executionResults.Add(ExecutionResult.Fail(
                        ledgerId, ExecutionStage.Ledger, i, ledgerResult.ErrorMessage!));
                    break;
                }

                executionResults.Add(ExecutionResult.Ok(ledgerId, ExecutionStage.Ledger, i));

                // Step 2: Settlement
                var settlementId = _idGenerator.DeterministicGuid(correlationId, "settlement", i.ToString());
                var settlementCtx = context with
                {
                    Action = "ExecuteSettlement",
                    CorrelationId = legCorrelation
                };

                var sharedMoney = new Whycespace.Shared.Primitives.Money.Money(
                    leg.Amount,
                    new Whycespace.Shared.Primitives.Money.Currency(leg.CurrencyCode));

                var createSettlement = await _ledgerService.CreateSettlementAsync(
                    settlementCtx, settlementId.ToString(), leg.ToSpvId.ToString(), sharedMoney);

                if (!createSettlement.Success)
                {
                    executionResults.Add(ExecutionResult.Fail(
                        settlementId, ExecutionStage.Settlement, i, createSettlement.ErrorMessage!));
                    break;
                }

                var ledgerEntries = new List<LedgerEntryDto>
                {
                    new(leg.FromSpvId, leg.Amount, "Debit", Guid.Parse(transactionId)),
                    new(leg.ToSpvId, leg.Amount, "Credit", Guid.Parse(transactionId))
                };

                var settleResult = await _ledgerService.ExecuteSettlementAsync(
                    settlementCtx, settlementId.ToString(), Guid.Parse(transactionId),
                    ledgerEntries, sharedMoney);

                if (!settleResult.Success)
                {
                    executionResults.Add(ExecutionResult.Fail(
                        settlementId, ExecutionStage.Settlement, i, settleResult.ErrorMessage!));
                    break;
                }

                executionResults.Add(ExecutionResult.Ok(settlementId, ExecutionStage.Settlement, i));
            }
        }
        catch (Exception ex)
        {
            executionResults.Add(ExecutionResult.Fail(
                Guid.Empty, ExecutionStage.Ledger, executionResults.Count, ex.Message));
        }

        // ── PHASE 3: COMMIT or COMPENSATE ────────────────────────
        var hasFailure = executionResults.Any(r => !r.Success);

        if (hasFailure)
        {
            // COMPENSATE: reverse all successful steps
            await _crossSpvService.SetExecutionStateAsync(
                context with { Action = "SetExecutionState" },
                id, CrossSpvExecutionState.Compensating.Value);

            await _compensationEngine.CompensateAsync(executionResults, context, ct);

            var failureReason = executionResults.First(r => !r.Success).ErrorMessage
                ?? "Execution failed";
            await FailTransaction(context, id, transactionId, failureReason);

            return CrossSpvProcessResult.Fail(failureReason);
        }

        // COMMIT: all legs succeeded
        await _crossSpvService.SetExecutionStateAsync(
            context with { Action = "SetExecutionState" },
            id, CrossSpvExecutionState.Committing.Value);

        var commitCtx = context with { Action = "CommitCrossSpvTransaction" };
        var commitResult = await _crossSpvService.CommitCrossSpvTransactionAsync(
            commitCtx, id, transactionId);

        if (!commitResult.Success)
            return CrossSpvProcessResult.Fail(commitResult.ErrorMessage ?? "Commit failed");

        return CrossSpvProcessResult.Ok(Guid.Parse(id));
    }

    private async Task FailTransaction(
        DomainExecutionContext context, string id, string transactionId, string reason)
    {
        var failCtx = context with { Action = "FailCrossSpvTransaction" };
        await _crossSpvService.FailCrossSpvTransactionAsync(failCtx, id, transactionId, reason);
    }
}

public sealed record CrossSpvProcessResult
{
    public bool Success { get; init; }
    public Guid? TransactionId { get; init; }
    public string? ErrorMessage { get; init; }

    public static CrossSpvProcessResult Ok(Guid transactionId) => new()
    {
        Success = true,
        TransactionId = transactionId
    };

    public static CrossSpvProcessResult Fail(string error) => new()
    {
        Success = false,
        ErrorMessage = error
    };
}
