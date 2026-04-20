using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Globalization;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Runtime.Admin;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Runtime.ControlPlane.Admin;

/// <summary>
/// R4.B / R-ADMIN-AUDIT-EMISSION-01 — routes <see cref="OperatorActionEvent"/>
/// envelopes onto the dedicated runtime operator-action audit stream via
/// <see cref="IEventFabric.ProcessAuditAsync"/>. Mirrors the constitutional
/// policy-decision eventification contract: deterministic event id, routing
/// overrides via <see cref="AuditEmission"/>, metadata-rich envelope.
///
/// <para>Every mutating admin controller action calls this recorder exactly
/// once per outcome branch — accepted, refused, or failed — so the audit
/// trail is complete even when the operator's request is rejected.</para>
///
/// <para>R4.A / R-OBS-OPERATOR-ACTION-METRIC-01 — every recorded event also
/// increments the <c>whyce.runtime.operator.action.total</c> counter on the
/// <c>Whycespace.Runtime.ControlPlane</c> meter, tagged by <c>action_type</c>
/// and <c>outcome</c>. Bounded low-cardinality (action types are a fixed
/// constant set; outcomes are three values). This is the single
/// R4.A-packaging-discovered gap: without it the runtime-control-plane
/// dashboard has no operator-action panel. All other dashboards are built
/// on pre-existing metrics.</para>
/// </summary>
public sealed class OperatorActionAuditRecorder : IOperatorActionRecorder
{
    // R4.A / R-OBS-OPERATOR-ACTION-METRIC-01 — shared meter with the
    // command-dispatch pipeline (Whycespace.Runtime.ControlPlane). Operator
    // actions are a control-plane concern; reusing the existing meter avoids
    // fragmenting the runtime observability surface.
    public const string MeterName = "Whycespace.Runtime.ControlPlane";
    private static readonly Meter OperatorActionMeter = new(MeterName, "1.0.0");
    private static readonly Counter<long> OperatorActionTotal =
        OperatorActionMeter.CreateCounter<long>("whyce.runtime.operator.action.total");

    private readonly IEventFabric _eventFabric;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public OperatorActionAuditRecorder(
        IEventFabric eventFabric,
        IIdGenerator idGenerator,
        IClock clock)
    {
        _eventFabric = eventFabric;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    public async Task<OperatorActionEvent> RecordAsync(
        string actionType,
        Guid targetId,
        string targetResourceType,
        string operatorIdentityId,
        string tenantId,
        Guid correlationId,
        string outcome,
        string? rationale = null,
        string? failureReason = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actionType))
            throw new ArgumentException("actionType is required.", nameof(actionType));
        if (string.IsNullOrWhiteSpace(targetResourceType))
            throw new ArgumentException("targetResourceType is required.", nameof(targetResourceType));
        if (string.IsNullOrWhiteSpace(operatorIdentityId))
            throw new ArgumentException("operatorIdentityId is required.", nameof(operatorIdentityId));
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("tenantId is required.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(outcome))
            throw new ArgumentException("outcome is required.", nameof(outcome));

        // R5.A / R-TRACE-OPERATOR-ACTION-SPAN-01 — wrap the audit emission
        // in a canonical runtime.admin.operator_action span. Span name is
        // FIXED; action_type + target_resource_type + outcome live on
        // attributes. Emitted on every outcome branch (accepted / refused /
        // failed) so refusal paths also leave a span trail.
        using var activity = WhyceActivitySources.Admin.StartActivity(
            WhyceActivitySources.Spans.OperatorAction,
            ActivityKind.Internal);
        activity?.SetTag(WhyceActivitySources.Attributes.ActionType, actionType);
        activity?.SetTag(WhyceActivitySources.Attributes.TargetResourceType, targetResourceType);
        activity?.SetTag(WhyceActivitySources.Attributes.TargetId, targetId);
        activity?.SetTag(WhyceActivitySources.Attributes.Outcome, outcome);
        activity?.SetTag(WhyceActivitySources.Attributes.ActorId, operatorIdentityId);
        activity?.SetTag(WhyceActivitySources.Attributes.TenantId, tenantId);
        activity?.SetTag(WhyceActivitySources.Attributes.CorrelationId, correlationId);
        if (!string.IsNullOrEmpty(failureReason))
            activity?.SetTag(WhyceActivitySources.Attributes.FailureReason, failureReason);
        activity?.SetStatus(
            outcome == OperatorActionOutcomes.Accepted
                ? ActivityStatusCode.Ok
                : ActivityStatusCode.Error,
            outcome);

        var eventId = _idGenerator.Generate(
            $"operator-action:{correlationId}:{actionType}:{targetId}");

        var evt = new OperatorActionEvent
        {
            EventId = eventId,
            OperatorIdentityId = operatorIdentityId,
            ActionType = actionType,
            TargetId = targetId,
            TargetResourceType = targetResourceType,
            Rationale = rationale,
            Outcome = outcome,
            FailureReason = failureReason,
            CorrelationId = correlationId,
            TenantId = tenantId,
            OccurredAt = _clock.UtcNow,
        };

        // Audit envelope aggregate id = event id so the audit stream keys by
        // the action envelope itself rather than the target aggregate. This
        // preserves domain-stream purity (a reconcile-target aggregate stream
        // never carries admin envelopes) per the constitutional routing rule.
        var audit = new AuditEmission
        {
            Events = new object[] { evt },
            AggregateId = eventId,
            Classification = AdminScope.AuditClassification,
            Context = AdminScope.AuditContext,
            Domain = AdminScope.AuditDomain,
            Metadata = new Dictionary<string, string>
            {
                ["ActionType"] = actionType,
                ["TargetResourceType"] = targetResourceType,
                ["TargetId"] = targetId.ToString(),
                ["OperatorIdentityId"] = operatorIdentityId,
                ["Outcome"] = outcome,
                ["CorrelationId"] = correlationId.ToString(),
                ["TenantId"] = tenantId,
                ["EventId"] = eventId.ToString(),
                // Observability timestamp — not a deterministic-hash contributor,
                // but recorded so downstream consumers can render the event
                // without re-deriving it.
                ["OccurredAt"] = evt.OccurredAt.ToString("O", CultureInfo.InvariantCulture),
            },
        };

        var context = new CommandContext
        {
            CorrelationId = correlationId,
            CausationId = correlationId,
            CommandId = eventId,
            TenantId = tenantId,
            ActorId = operatorIdentityId,
            AggregateId = eventId,
            PolicyId = "runtime/operator-action",
            Classification = AdminScope.AuditClassification,
            Context = AdminScope.AuditContext,
            Domain = AdminScope.AuditDomain,
            IsSystem = false,
        };

        await _eventFabric.ProcessAuditAsync(audit, context, cancellationToken);

        // R4.A / R-OBS-OPERATOR-ACTION-METRIC-01 — emit AFTER the fabric
        // accepts the audit envelope so the counter reflects durably-recorded
        // actions only. Fabric-throw paths skip the counter but the exception
        // propagates to the controller, which will fail the response.
        OperatorActionTotal.Add(1,
            new KeyValuePair<string, object?>("action_type", actionType),
            new KeyValuePair<string, object?>("outcome", outcome));

        return evt;
    }
}
