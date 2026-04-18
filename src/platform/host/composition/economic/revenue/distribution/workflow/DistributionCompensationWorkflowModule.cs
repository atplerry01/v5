using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T1M.Domains.Economic.Revenue.Distribution.Steps;
using Whycespace.Shared.Contracts.Economic.Revenue.Distribution.Workflow;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Economic.Revenue.Distribution.Workflow;

/// <summary>
/// Phase 7 B2 — DistributionCompensationWorkflow composition module.
/// Registers the T1M compensation steps and binds them to
/// <see cref="DistributionCompensationWorkflowNames.Compensate"/>.
///
/// Canonical step chain (LOCKED — T7.3):
///   RequestDistributionCompensationStep → MarkDistributionCompensatedStep
///
/// This workflow is chained from <c>PayoutCompensationWorkflow</c> once
/// the compensating ledger journal has been posted successfully; the
/// compensating journal id is carried forward in
/// <see cref="DistributionCompensationIntent.CompensatingJournalId"/>
/// so the terminal distribution transition stays correlated to the
/// ledger reversal.
///
/// T2E handlers backing the compensation commands
/// (<c>RequestDistributionCompensationHandler</c>,
/// <c>MarkDistributionCompensatedHandler</c>) are registered by
/// <c>DistributionWorkflowModule</c>.
/// </summary>
public static class DistributionCompensationWorkflowModule
{
    public static IServiceCollection AddDistributionCompensationWorkflow(this IServiceCollection services)
    {
        services.AddTransient<RequestDistributionCompensationStep>();
        services.AddTransient<MarkDistributionCompensatedStep>();
        return services;
    }

    public static void RegisterWorkflows(IWorkflowRegistry workflow)
    {
        workflow.Register(DistributionCompensationWorkflowNames.Compensate, new[]
        {
            typeof(RequestDistributionCompensationStep),
            typeof(MarkDistributionCompensatedStep)
        });
    }
}
