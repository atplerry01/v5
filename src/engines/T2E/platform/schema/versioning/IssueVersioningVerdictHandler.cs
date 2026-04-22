using Whycespace.Domain.PlatformSystem.Schema.Versioning;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Schema.Versioning;

namespace Whycespace.Engines.T2E.Platform.Schema.Versioning;

public sealed class IssueVersioningVerdictHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not IssueVersioningVerdictCommand cmd)
            return;

        var aggregate = (VersioningRuleAggregate)await context.LoadAggregateAsync(typeof(VersioningRuleAggregate));

        var verdict = cmd.Verdict switch
        {
            "ConditionallyCompatible" => CompatibilityVerdict.ConditionallyCompatible,
            "Incompatible" => CompatibilityVerdict.Incompatible,
            _ => CompatibilityVerdict.Compatible
        };

        aggregate.IssueVerdict(verdict, new Timestamp(cmd.IssuedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
