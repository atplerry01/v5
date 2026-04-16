using Whycespace.Domain.EconomicSystem.Enforcement.Restriction;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Enforcement.Restriction;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Enforcement.Restriction;

public sealed class ApplyRestrictionHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ApplyRestrictionCommand cmd)
            return Task.CompletedTask;

        if (!Enum.TryParse<RestrictionScope>(cmd.Scope, ignoreCase: true, out var scope))
            throw new InvalidOperationException($"Unknown restriction scope: '{cmd.Scope}'.");

        var aggregate = RestrictionAggregate.Apply(
            new RestrictionId(cmd.RestrictionId),
            new SubjectId(cmd.SubjectId),
            scope,
            new Reason(cmd.Reason),
            new Timestamp(cmd.AppliedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
