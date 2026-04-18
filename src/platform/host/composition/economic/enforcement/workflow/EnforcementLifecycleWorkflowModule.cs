using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T1M.Domains.Economic.Enforcement.Steps;
using Whycespace.Shared.Contracts.Economic.Enforcement.Workflow;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Economic.Enforcement.Workflow;

/// <summary>
/// Enforcement lifecycle workflow module — T1M step DI registrations and
/// workflow registry binding for "economic.enforcement.lifecycle".
///
/// Workflow chain: violation → escalation → sanction → restriction → lock.
/// Deterministic, idempotent, event-driven.
///
/// Severity-tiered progression:
///   - Low/Medium:  escalate + sanction (restriction/lock are no-ops).
///   - High:        escalate + sanction + restriction (lock is no-op).
///   - Critical:    escalate + sanction + restriction + lock.
/// </summary>
public static class EnforcementLifecycleWorkflowModule
{
    public static IServiceCollection AddEnforcementLifecycleWorkflow(this IServiceCollection services)
    {
        services.AddTransient<EscalateViolationStep>();
        services.AddTransient<IssueSanctionStep>();
        services.AddTransient<ApplyRestrictionStep>();
        services.AddTransient<LockSystemStep>();
        return services;
    }

    public static void RegisterWorkflows(IWorkflowRegistry workflow)
    {
        workflow.Register(EnforcementLifecycleWorkflowNames.Lifecycle, new[]
        {
            typeof(EscalateViolationStep),
            typeof(IssueSanctionStep),
            typeof(ApplyRestrictionStep),
            typeof(LockSystemStep)
        });
    }
}
