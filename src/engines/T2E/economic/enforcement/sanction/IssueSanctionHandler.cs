using Whycespace.Domain.EconomicSystem.Enforcement.Sanction;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Enforcement.Sanction;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Enforcement.Sanction;

public sealed class IssueSanctionHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not IssueSanctionCommand cmd)
            return Task.CompletedTask;

        if (!Enum.TryParse<SanctionType>(cmd.Type, ignoreCase: true, out var type))
            throw new InvalidOperationException($"Unknown sanction type: '{cmd.Type}'.");
        if (!Enum.TryParse<SanctionScope>(cmd.Scope, ignoreCase: true, out var scope))
            throw new InvalidOperationException($"Unknown sanction scope: '{cmd.Scope}'.");

        var period = cmd.ExpiresAt.HasValue
            ? EffectivePeriod.Bounded(new Timestamp(cmd.EffectiveAt), new Timestamp(cmd.ExpiresAt.Value))
            : EffectivePeriod.Open(new Timestamp(cmd.EffectiveAt));

        var aggregate = SanctionAggregate.Issue(
            new SanctionId(cmd.SanctionId),
            new SubjectId(cmd.SubjectId),
            type,
            scope,
            new Reason(cmd.Reason),
            period,
            new Timestamp(cmd.IssuedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
