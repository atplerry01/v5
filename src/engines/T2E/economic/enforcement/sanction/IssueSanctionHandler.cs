using Whycespace.Domain.ControlSystem.Enforcement.Sanction;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Enforcement.Sanction;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;

namespace Whycespace.Engines.T2E.Economic.Enforcement.Sanction;

/// <summary>
/// Phase 7 T7.11 — idempotent issuance. A duplicate
/// <see cref="IssueSanctionCommand"/> on a SanctionId that already has an
/// event stream collapses to no-op so at-least-once command redelivery
/// never produces a second <see cref="SanctionIssuedEvent"/>.
/// </summary>
public sealed class IssueSanctionHandler : IEngine
{
    private readonly IEventStore _eventStore;

    public IssueSanctionHandler(IEventStore eventStore) => _eventStore = eventStore;

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not IssueSanctionCommand cmd)
            return;

        // Idempotent replay: if this sanction stream already exists,
        // issuance has already happened — emit nothing and let the
        // dispatcher return CommandResult.Success([]).
        var prior = await _eventStore.LoadEventsAsync(context.AggregateId);
        if (prior.Count > 0)
            return;

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
    }
}
