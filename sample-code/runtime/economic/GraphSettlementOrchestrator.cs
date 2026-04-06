using Whycespace.Runtime.Economic.Mapping;
using Whycespace.Runtime.Economic.Result;
using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Economic;
using Whycespace.Shared.Contracts.Domain.Economic.Ledger;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Money;

namespace Whycespace.Runtime.Economic;

/// <summary>
/// Graph-aware settlement orchestrator (E17.6).
/// Connects graph-routed economic flows (E17.5) to existing
/// ledger + settlement domains via ILedgerDomainService.
///
/// Flow: Graph Path → Ledger Entries → Double-Entry Posting → Settlement
///
/// CRITICAL: Orchestrates only — business invariants enforced by domain.
/// </summary>
public sealed class GraphSettlementOrchestrator
{
    private readonly ILedgerDomainService _ledgerService;

    public GraphSettlementOrchestrator(ILedgerDomainService ledgerService)
    {
        _ledgerService = ledgerService;
    }

    public async Task<GraphSettlementResult> ExecuteAsync(
        IEconomicGraphContext context,
        DomainExecutionContext executionContext,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(executionContext);

        if (context.ExecutionPath == null || context.ExecutionPath.Count == 0)
            return GraphSettlementResult.Failed("Empty execution path");

        // Step 1: Generate deterministic transaction ID from context
        var transactionId = DeterministicTransactionId(context);

        // Step 2: Map graph hops to balanced ledger entries
        var ledgerEntries = GraphToLedgerMapper.Map(context, transactionId);

        if (ledgerEntries.Count == 0)
            return GraphSettlementResult.Failed("No ledger entries generated from execution path");

        // Step 3: Post double-entry via existing ledger domain
        var nodes = context.ExecutionPath.ToList();
        for (var i = 0; i < nodes.Count - 1; i++)
        {
            var entryId = DeterministicEntryId(transactionId, i);
            var postResult = await _ledgerService.RecordDoubleEntryAsync(
                executionContext,
                transactionId.ToString(),
                entryId.ToString(),
                nodes[i].ToString(),
                $"Entity-{nodes[i]:N}",
                context.Amount,
                context.Amount,
                context.Currency);

            if (!postResult.Success)
                return GraphSettlementResult.Failed(postResult.ErrorMessage ?? "Ledger posting failed");
        }

        // Step 4: Create and execute settlement via existing domain
        var settlementId = DeterministicSettlementId(transactionId);
        var amount = new Money(context.Amount, new Currency(context.Currency));

        var createResult = await _ledgerService.CreateSettlementAsync(
            executionContext,
            settlementId.ToString(),
            context.SourceEntityId.ToString(),
            amount);

        if (!createResult.Success)
            return GraphSettlementResult.Failed(createResult.ErrorMessage ?? "Settlement creation failed");

        var settleResult = await _ledgerService.ExecuteSettlementAsync(
            executionContext,
            settlementId.ToString(),
            transactionId,
            ledgerEntries,
            amount);

        if (!settleResult.Success)
            return GraphSettlementResult.Failed(settleResult.ErrorMessage ?? "Settlement execution failed");

        return GraphSettlementResult.Success(transactionId);
    }

    private static Guid DeterministicTransactionId(IEconomicGraphContext context)
    {
        return DeterministicIdHelper.FromSeed(
            $"{context.SourceEntityId}:{context.TargetEntityId}:{context.Amount}:{context.Currency}");
    }

    private static Guid DeterministicEntryId(Guid transactionId, int index)
    {
        return DeterministicIdHelper.FromSeed($"{transactionId}:entry:{index}");
    }

    private static Guid DeterministicSettlementId(Guid transactionId)
    {
        return DeterministicIdHelper.FromSeed($"{transactionId}:settlement");
    }
}
