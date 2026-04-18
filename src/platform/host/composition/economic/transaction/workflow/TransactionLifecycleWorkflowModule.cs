using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T1M.Domains.Economic.Transaction.Steps;
using Whycespace.Shared.Contracts.Economic.Transaction.Workflow;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Economic.Transaction.Workflow;

/// <summary>
/// Transaction lifecycle workflow module — T1M step DI registrations and
/// workflow registry binding for "economic.transaction.lifecycle".
///
/// Phase 4 (LOCKED) chain:
///   ValidateLifecycleIntent → ExecuteInstruction → InitiateTransaction
///   → CheckLimit → InitiateSettlement → FxLock → PostToLedger.
///
/// Control plane invariants:
///   * CheckLimit runs AFTER InitiateTransaction and BEFORE InitiateSettlement
///     and PostToLedger. No transaction can post to the ledger without
///     passing the per-account limit gate.
///   * FxLock runs AFTER InitiateSettlement and BEFORE PostToLedger so the
///     ledger entries always carry a deterministic FX rate snapshot for
///     cross-currency transactions (no-op for single-currency).
///   * Hard-block on limit breach is automatic: CheckLimitStep returns
///     Failure on a non-success CommandResult, and the workflow halts
///     before settlement / ledger steps execute.
/// </summary>
public static class TransactionLifecycleWorkflowModule
{
    public static IServiceCollection AddTransactionLifecycleWorkflow(this IServiceCollection services)
    {
        services.AddTransient<ValidateLifecycleIntentStep>();
        services.AddTransient<ExecuteInstructionStep>();
        services.AddTransient<InitiateTransactionStep>();
        services.AddTransient<CheckLimitStep>();
        services.AddTransient<InitiateSettlementStep>();
        services.AddTransient<FxLockStep>();
        services.AddTransient<PostToLedgerStep>();
        return services;
    }

    public static void RegisterWorkflows(IWorkflowRegistry workflow)
    {
        workflow.Register(TransactionLifecycleWorkflowNames.Lifecycle, new[]
        {
            typeof(ValidateLifecycleIntentStep),
            typeof(ExecuteInstructionStep),
            typeof(InitiateTransactionStep),
            typeof(CheckLimitStep),
            typeof(InitiateSettlementStep),
            typeof(FxLockStep),
            typeof(PostToLedgerStep)
        });
    }
}
