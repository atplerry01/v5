using Whycespace.Domain.ControlSystem.Enforcement.Rule;
using Whycespace.Domain.SharedKernel.Primitive.Identity;
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

        if (!ContentId.TryParse(cmd.Description, out var descriptionContentId))
            throw new InvalidOperationException(
                "DefineEnforcementRuleCommand.Description must be a non-empty GUID identifying the description content aggregate (ContentId).");

        var aggregate = EnforcementRuleAggregate.Define(
            new RuleId(cmd.RuleId),
            new RuleCode(cmd.RuleCode),
            new RuleName(cmd.RuleName),
            new RuleCategory(cmd.RuleCategory),
            scope,
            severity,
            new DocumentRef(descriptionContentId),
            new Timestamp(cmd.CreatedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
