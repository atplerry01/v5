using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Contracts.Policy;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.ControlPlane.Policy;

/// <summary>
/// Routes enforcement actions to correct system targets via command events.
/// Uses EventBus abstraction — no direct system calls.
/// Idempotent: deterministic action IDs prevent duplicate dispatch.
/// </summary>
public sealed class PolicyEnforcementDispatcher
{
    private readonly IEventPublisher _eventPublisher;
    private readonly IClock _clock;
    private readonly HashSet<Guid> _dispatchedActions = [];

    public PolicyEnforcementDispatcher(IEventPublisher eventPublisher, IClock clock)
    {
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task DispatchAsync(
        IReadOnlyList<EnforcementActionDto> actions,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actions);

        foreach (var action in actions)
        {
            if (!_dispatchedActions.Add(action.ActionId))
                continue;

            var commandType = $"enforcement.{action.TargetType}.{action.Type}";

            await _eventPublisher.PublishAsync(new RuntimeEvent
            {
                EventId = DeterministicIdHelper.FromSeed($"policy-enforcement:{action.ActionId}:{action.CorrelationId}"),
                AggregateId = Guid.Empty,
                AggregateType = "policy.enforcement",
                EventType = commandType,
                CorrelationId = action.CorrelationId,
                Payload = new
                {
                    action.ActionId, action.Type, action.Severity,
                    action.TargetType, action.TargetId, action.Reason
                },
                Timestamp = _clock.UtcNowOffset
            }, cancellationToken);
        }
    }
}
