using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T1M.Domains.Economic.Revenue.Payout.Steps;
using Whycespace.Shared.Contracts.Economic.Revenue.Payout.Workflow;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Economic.Revenue.Payout.Workflow;

/// <summary>
/// Phase 7 B2 — PayoutCompensationWorkflow composition module. Registers
/// the T1M compensation steps and binds them to the canonical workflow
/// name <see cref="PayoutCompensationWorkflowNames.Compensate"/> so
/// <c>IWorkflowDispatcher</c> can start the saga deterministically in
/// response to a payout failure path.
///
/// Canonical step chain (LOCKED — T7.3/T7.4):
///   RequestPayoutCompensationStep
///     → PostCompensatingLedgerJournalStep (T7.4 append-only reversal)
///     → MarkPayoutCompensatedStep
///
/// The T2E handlers backing the compensation commands
/// (<c>RequestPayoutCompensationHandler</c>,
/// <c>MarkPayoutCompensatedHandler</c>) are registered by
/// <c>PayoutCompositionModule</c>. This module owns only the T1M step
/// DI + workflow-registry binding.
/// </summary>
public static class PayoutCompensationWorkflowModule
{
    public static IServiceCollection AddPayoutCompensationWorkflow(this IServiceCollection services)
    {
        services.AddTransient<RequestPayoutCompensationStep>();
        services.AddTransient<PostCompensatingLedgerJournalStep>();
        services.AddTransient<MarkPayoutCompensatedStep>();
        return services;
    }

    public static void RegisterWorkflows(IWorkflowRegistry workflow)
    {
        workflow.Register(PayoutCompensationWorkflowNames.Compensate, new[]
        {
            typeof(RequestPayoutCompensationStep),
            typeof(PostCompensatingLedgerJournalStep),
            typeof(MarkPayoutCompensatedStep)
        });
    }
}
