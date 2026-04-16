using Microsoft.Extensions.DependencyInjection;
using Whycespace.Shared.Contracts.Economic.Reconciliation.Discrepancy;
using Whycespace.Shared.Contracts.Economic.Reconciliation.Process;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Economic.Reconciliation;

public static class ReconciliationPolicyModule
{
    public static IServiceCollection AddReconciliationPolicyBindings(this IServiceCollection services)
    {
        // ── process (4) ────────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(TriggerReconciliationCommand), ReconciliationProcessPolicyIds.Trigger));
        services.AddSingleton(new CommandPolicyBinding(typeof(MarkMatchedCommand),           ReconciliationProcessPolicyIds.Matched));
        services.AddSingleton(new CommandPolicyBinding(typeof(MarkMismatchedCommand),        ReconciliationProcessPolicyIds.Mismatched));
        services.AddSingleton(new CommandPolicyBinding(typeof(ResolveReconciliationCommand), ReconciliationProcessPolicyIds.Resolve));

        // ── discrepancy (3) ────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(DetectDiscrepancyCommand),      ReconciliationDiscrepancyPolicyIds.Detect));
        services.AddSingleton(new CommandPolicyBinding(typeof(InvestigateDiscrepancyCommand), ReconciliationDiscrepancyPolicyIds.Investigate));
        services.AddSingleton(new CommandPolicyBinding(typeof(ResolveDiscrepancyCommand),     ReconciliationDiscrepancyPolicyIds.Resolve));

        return services;
    }
}
