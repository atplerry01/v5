using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T1M.Domains.Economic.Revenue.Payout.Steps;
using Whycespace.Shared.Contracts.Economic.Revenue.Payout.Workflow;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Economic.Revenue.Payout.Workflow;

/// <summary>
/// Payout execution workflow module — T1M step DI registrations and workflow
/// registry binding for "economic.payout.execute". Reuses the
/// DebitSliceHandler / CreditSliceHandler wired by VaultAccountApplicationModule.
///
/// Phase 3 (LOCKED): chain is
///   EnsureContractActive → LoadDistribution → RequestPayout → ExecutePayout
///   → MarkPayoutExecuted → PostLedgerJournal → MarkDistributionPaid.
/// PostLedgerJournalStep is the explicit T3.5 emission point —
/// LedgerJournalPostedEvent is produced once per payout, with full journal
/// reference (ledger id, journal id, balanced entries).
/// </summary>
public static class PayoutExecutionWorkflowModule
{
    public static IServiceCollection AddPayoutExecutionWorkflow(this IServiceCollection services)
    {
        services.AddTransient<EnsureContractActiveStep>();
        services.AddTransient<LoadDistributionStep>();
        services.AddTransient<RequestPayoutStep>();
        services.AddTransient<ExecutePayoutStep>();
        services.AddTransient<MarkPayoutExecutedStep>();
        services.AddTransient<PostLedgerJournalStep>();
        services.AddTransient<MarkDistributionPaidStep>();
        return services;
    }

    public static void RegisterWorkflows(IWorkflowRegistry workflow)
    {
        workflow.Register(PayoutExecutionWorkflowNames.Execute, new[]
        {
            typeof(EnsureContractActiveStep),
            typeof(LoadDistributionStep),
            typeof(RequestPayoutStep),
            typeof(ExecutePayoutStep),
            typeof(MarkPayoutExecutedStep),
            typeof(PostLedgerJournalStep),
            typeof(MarkDistributionPaidStep)
        });
    }
}
