using Microsoft.Extensions.DependencyInjection;
using Whycespace.Shared.Contracts.Economic.Enforcement.Escalation;
using Whycespace.Shared.Contracts.Economic.Enforcement.Lock;
using Whycespace.Shared.Contracts.Economic.Enforcement.Restriction;
using Whycespace.Shared.Contracts.Economic.Enforcement.Rule;
using Whycespace.Shared.Contracts.Economic.Enforcement.Sanction;
using Whycespace.Shared.Contracts.Economic.Enforcement.Violation;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Economic.Enforcement;

public static class EnforcementPolicyModule
{
    public static IServiceCollection AddEnforcementPolicyBindings(this IServiceCollection services)
    {
        services.AddSingleton(new CommandPolicyBinding(typeof(DefineEnforcementRuleCommand),   EnforcementRulePolicyIds.Define));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateEnforcementRuleCommand), EnforcementRulePolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(DisableEnforcementRuleCommand),  EnforcementRulePolicyIds.Disable));
        services.AddSingleton(new CommandPolicyBinding(typeof(RetireEnforcementRuleCommand),   EnforcementRulePolicyIds.Retire));

        services.AddSingleton(new CommandPolicyBinding(typeof(DetectViolationCommand),         ViolationPolicyIds.Detect));
        services.AddSingleton(new CommandPolicyBinding(typeof(AcknowledgeViolationCommand),    ViolationPolicyIds.Acknowledge));
        services.AddSingleton(new CommandPolicyBinding(typeof(ResolveViolationCommand),        ViolationPolicyIds.Resolve));

        services.AddSingleton(new CommandPolicyBinding(typeof(AccumulateViolationCommand),     EscalationPolicyIds.Accumulate));

        services.AddSingleton(new CommandPolicyBinding(typeof(IssueSanctionCommand),           SanctionPolicyIds.Issue));
        services.AddSingleton(new CommandPolicyBinding(typeof(ActivateSanctionCommand),        SanctionPolicyIds.Activate));
        services.AddSingleton(new CommandPolicyBinding(typeof(RevokeSanctionCommand),          SanctionPolicyIds.Revoke));
        services.AddSingleton(new CommandPolicyBinding(typeof(ExpireSanctionCommand),          SanctionPolicyIds.Expire));

        services.AddSingleton(new CommandPolicyBinding(typeof(ApplyRestrictionCommand),        RestrictionPolicyIds.Apply));
        services.AddSingleton(new CommandPolicyBinding(typeof(UpdateRestrictionCommand),       RestrictionPolicyIds.Update));
        services.AddSingleton(new CommandPolicyBinding(typeof(RemoveRestrictionCommand),       RestrictionPolicyIds.Remove));

        services.AddSingleton(new CommandPolicyBinding(typeof(LockSystemCommand),              LockPolicyIds.Lock));
        services.AddSingleton(new CommandPolicyBinding(typeof(UnlockSystemCommand),            LockPolicyIds.Unlock));
        return services;
    }
}
