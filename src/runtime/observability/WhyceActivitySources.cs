using System.Diagnostics;

namespace Whycespace.Runtime.Observability;

/// <summary>
/// R5.A / R-TRACE-SOURCE-VOCABULARY-01 — canonical <see cref="ActivitySource"/>
/// registry + span-name constants for the runtime tracing pipeline.
///
/// <para>Runtime-layer code emits spans via these static handles; the host
/// registers them on <c>TracerProvider</c> (see
/// <c>TracingInfrastructureModule</c>). Keeping the registry here preserves
/// layer discipline — <c>Whycespace.Runtime</c> depends only on
/// <see cref="System.Diagnostics"/> stdlib; OTEL NuGet refs live
/// exclusively in the host project (DG-R5-01).</para>
///
/// <para><b>Span-name discipline:</b> span names are low-cardinality
/// canonical strings. Per-request dimensions (command type, workflow name,
/// action type, aggregate id, correlation id) go on span <b>attributes</b>,
/// never in the span name. Trace backends can filter on attributes;
/// unbounded span-name cardinality breaks backend storage.</para>
/// </summary>
public static class WhyceActivitySources
{
    /// <summary>
    /// Control-plane span source. Wraps command dispatch, enforcement
    /// decisions, and any other middleware-level execution that is part
    /// of the canonical runtime command pipeline.
    /// </summary>
    public const string ControlPlaneName = "Whycespace.Runtime.ControlPlane";

    /// <summary>
    /// Admin / operator-action span source. Wraps R4.B admin surface
    /// mutations (reconcile, redrive, workflow resume/approve/reject)
    /// and their audit-emission side effect.
    /// </summary>
    public const string AdminName = "Whycespace.Runtime.Admin";

    /// <summary>
    /// R5.A Phase 2 — event fabric span source. Wraps the canonical
    /// persist → chain → outbox orchestration (<c>IEventFabric.ProcessAsync</c> +
    /// <c>ProcessAuditAsync</c>).
    /// </summary>
    public const string EventFabricName = "Whycespace.Runtime.EventFabric";

    /// <summary>
    /// R5.A Phase 2 — outbound-effect span source. Wraps the canonical
    /// scheduling seam (<c>IOutboundEffectDispatcher.ScheduleAsync</c>).
    /// The dispatch-loop + lifecycle-transition seams are Phase 3.
    /// </summary>
    public const string OutboundEffectsName = "Whycespace.Runtime.OutboundEffects";

    public static readonly ActivitySource ControlPlane = new(ControlPlaneName, "1.0.0");
    public static readonly ActivitySource Admin = new(AdminName, "1.0.0");
    public static readonly ActivitySource EventFabric = new(EventFabricName, "1.0.0");
    public static readonly ActivitySource OutboundEffects = new(OutboundEffectsName, "1.0.0");

    /// <summary>
    /// Canonical span names. Extending this list requires promoting the new
    /// name into <c>runtime.guard.md</c> §R5.A and the
    /// <c>RuntimeTracingVocabularyTests</c> allowlist.
    /// </summary>
    public static class Spans
    {
        /// <summary>
        /// Wraps <c>SystemIntentDispatcher.DispatchAsync</c> — one span per
        /// command dispatch. Attributes carry the canonical routing
        /// coordinates (command.type, classification, context, domain)
        /// plus aggregate.id / actor.id / correlation.id for drill-down.
        /// </summary>
        public const string CommandDispatch = "runtime.command.dispatch";

        /// <summary>
        /// Wraps <c>OperatorActionAuditRecorder.RecordAsync</c> — one span
        /// per operator action. Attributes carry action_type,
        /// target_resource_type, target.id, outcome, operator.id.
        /// </summary>
        public const string OperatorAction = "runtime.admin.operator_action";

        /// <summary>
        /// R5.A Phase 2 — wraps <c>EventFabric.ProcessAsync</c>. One span per
        /// persist → chain → outbox orchestration for a command's domain
        /// events. Attributes carry event_count, aggregate.id, and the
        /// classification/context/domain routing triple.
        /// </summary>
        public const string EventFabricProcess = "event.fabric.process";

        /// <summary>
        /// R5.A Phase 2 — wraps <c>EventFabric.ProcessAuditAsync</c>. One
        /// span per audit-stream emission (policy decisions, operator
        /// actions). Distinct span name so operators can filter the audit
        /// stream separately from domain-aggregate streams in Jaeger.
        /// </summary>
        public const string EventFabricProcessAudit = "event.fabric.process_audit";

        /// <summary>
        /// R5.A Phase 2 — wraps <c>OutboundEffectDispatcher.ScheduleAsync</c>.
        /// One span per outbound-effect schedule. Attributes carry provider,
        /// effect_type, idempotency_key, and dedup_hit outcome.
        /// </summary>
        public const string OutboundEffectSchedule = "outbound.effect.schedule";

        /// <summary>
        /// R5.A Phase 3 — wraps <c>OutboundEffectRelay.DispatchOneAsync</c>.
        /// One span per dispatch ATTEMPT — a retrying effect produces
        /// multiple dispatch spans, which is the operator-useful shape for
        /// tracking retry behavior. Attributes carry provider, effect_type,
        /// attempt.number, outcome (success/failure/breaker_open/timeout/ambiguous).
        /// </summary>
        public const string OutboundEffectDispatch = "outbound.effect.dispatch";

        /// <summary>
        /// R5.A Phase 3 — wraps <c>OutboundEffectFinalityService.FinalizeAsync</c>.
        /// One span per finality transition (async callback / poll /
        /// synchronous ack completion). Attributes carry effect id,
        /// provider, outcome, finality source, compensation_emitted boolean.
        /// </summary>
        public const string OutboundEffectFinalize = "outbound.effect.finalize";

        /// <summary>
        /// R5.A Phase 3 — wraps <c>OutboundEffectFinalityService.ReconcileAsync</c>.
        /// One span per OPERATOR-DRIVEN reconcile (R4.B admin surface
        /// invocation). Attributes carry effect id, outcome, reconciler
        /// actor id, compensation_emitted boolean. The sweeper's
        /// <c>MarkReconciliationRequiredAsync</c> background transitions
        /// are intentionally NOT span-wrapped — background worker timers
        /// produce too much span noise.
        /// </summary>
        public const string OutboundEffectReconcile = "outbound.effect.reconcile";
    }

    /// <summary>
    /// Canonical span-attribute keys. Reuse these constants so the
    /// attribute vocabulary is consistent across instrumented seams.
    ///
    /// <para>Low-cardinality discipline applies to metric labels, not to
    /// span attributes — span attributes MAY include high-cardinality
    /// dimensions (correlation.id, aggregate.id, actor.id). The trace
    /// backend indexes them; metric aggregation does not.</para>
    /// </summary>
    public static class Attributes
    {
        public const string CommandType = "whyce.command.type";
        public const string Classification = "whyce.classification";
        public const string Context = "whyce.context";
        public const string Domain = "whyce.domain";
        public const string AggregateId = "whyce.aggregate.id";
        public const string ActorId = "whyce.actor.id";
        public const string TenantId = "whyce.tenant.id";
        public const string CorrelationId = "whyce.correlation_id";
        public const string ActionType = "whyce.action.type";
        public const string TargetResourceType = "whyce.target.resource_type";
        public const string TargetId = "whyce.target.id";
        public const string Outcome = "whyce.outcome";
        public const string FailureReason = "whyce.failure_reason";

        // R5.A Phase 2 additions — event-fabric + outbound-effect dimensions.
        public const string EventCount = "whyce.event.count";
        public const string ProviderId = "whyce.provider.id";
        public const string EffectType = "whyce.effect.type";
        public const string IdempotencyKey = "whyce.idempotency.key";
        public const string DedupHit = "whyce.dedup.hit";

        // R5.A Phase 3 additions — outbound-effect lifecycle.
        public const string AttemptNumber = "whyce.attempt.number";
        public const string FinalitySource = "whyce.finality.source";
        public const string CompensationEmitted = "whyce.compensation.emitted";
        public const string ReconcilerActorId = "whyce.reconciler.actor_id";
    }
}
