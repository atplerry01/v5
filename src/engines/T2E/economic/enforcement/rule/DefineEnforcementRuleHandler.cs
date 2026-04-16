using Whycespace.Domain.EconomicSystem.Enforcement.Rule;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Enforcement.Rule;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Enforcement.Rule;

public sealed class DefineEnforcementRuleHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DefineEnforcementRuleCommand cmd)
            return Task.CompletedTask;

        if (!Enum.TryParse<RuleScope>(cmd.Scope, ignoreCase: true, out var scope))
            throw new InvalidOperationException($"Unknown rule scope: '{cmd.Scope}'.");

        if (!Enum.TryParse<RuleSeverity>(cmd.Severity, ignoreCase: true, out var severity))
            throw new InvalidOperationException($"Unknown rule severity: '{cmd.Severity}'.");

        var aggregate = EnforcementRuleAggregate.Define(
            new RuleId(cmd.RuleId),
            new RuleCode(cmd.RuleCode),
            cmd.RuleName,
            new RuleCategory(cmd.RuleCategory),
            scope,
            severity,
            cmd.Description,
            new Timestamp(cmd.CreatedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
