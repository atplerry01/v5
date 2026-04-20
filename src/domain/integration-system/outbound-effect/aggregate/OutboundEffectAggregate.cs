using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.IntegrationSystem.OutboundEffect;

/// <summary>
/// R3.B.1 — event-sourced aggregate owning the lifecycle of a single outbound
/// effect. Authoritative source of truth; replay from the event stream
/// reconstructs the canonical status without external state.
///
/// <para><b>Construction discipline (R-OUT-EFF-SEAM-01):</b>
/// <see cref="Start(Guid,string,string,string,string?,string,int,int,int,int,int,object?)"/>
/// is the SOLE construction path. Callers MUST invoke it through
/// <c>OutboundEffectLifecycleEventFactory</c> (T2E). An architecture test
/// asserts no other file under <c>src/**</c> calls
/// <c>OutboundEffectAggregate.Start(</c>.</para>
///
/// <para><b>Mutation discipline (R-OUT-EFF-SEAM-03):</b> there are no public
/// mutator methods other than <see cref="Start"/>. The relay / dispatcher
/// construct events via the lifecycle factory and append them through the
/// canonical persist → chain → outbox pipeline; the aggregate reconstructs
/// state on replay via <see cref="Apply"/>.</para>
///
/// <para><b>Ratified constraint #1:</b> <c>Acknowledged</c> never implies
/// <c>Finalized</c>. The state machine preserves six distinct adapter
/// outcomes exactly; aggregate <see cref="Apply"/> refuses to collapse them.</para>
/// </summary>
public sealed class OutboundEffectAggregate : AggregateRoot
{
    public OutboundEffectId Id { get; private set; }

    private string _providerId = string.Empty;
    private string _effectType = string.Empty;
    private string _idempotencyKey = string.Empty;
    private string? _payloadTypeDiscriminator;
    private int _attemptCount;
    private int _maxAttempts;
    private int _dispatchTimeoutMs;
    private int _totalBudgetMs;
    private int _ackTimeoutMs;
    private int _finalityWindowMs;
    private OutboundEffectStatus _status = OutboundEffectStatus.Scheduled;
    private string? _providerOperationId;
    private string? _failureClassification;
    private string? _failureReason;
    private string? _finalityOutcome;
    private string? _reconciliationCause;

    public string ProviderId => _providerId;
    public string EffectType => _effectType;
    public string IdempotencyKey => _idempotencyKey;
    public string? PayloadTypeDiscriminator => _payloadTypeDiscriminator;
    public int AttemptCount => _attemptCount;
    public int MaxAttempts => _maxAttempts;
    public int DispatchTimeoutMs => _dispatchTimeoutMs;
    public int TotalBudgetMs => _totalBudgetMs;
    public int AckTimeoutMs => _ackTimeoutMs;
    public int FinalityWindowMs => _finalityWindowMs;
    public OutboundEffectStatus Status => _status;
    public string? ProviderOperationId => _providerOperationId;
    public string? FailureClassification => _failureClassification;
    public string? FailureReason => _failureReason;
    public string? FinalityOutcome => _finalityOutcome;
    public string? ReconciliationCause => _reconciliationCause;

    private OutboundEffectAggregate() { }

    /// <summary>
    /// R3.B.1 / R-OUT-EFF-SEAM-01 — sole construction path. Invoked exclusively
    /// from <c>OutboundEffectLifecycleEventFactory.Start(...)</c>. Architecture
    /// test pins the single-caller invariant.
    /// </summary>
    public static OutboundEffectAggregate Start(
        Guid effectId,
        string providerId,
        string effectType,
        string idempotencyKey,
        string? payloadTypeDiscriminator,
        string schedulerActorId,
        int dispatchTimeoutMs,
        int totalBudgetMs,
        int ackTimeoutMs,
        int finalityWindowMs,
        int maxAttempts,
        object? payload = null)
    {
        Guard.Against(string.IsNullOrWhiteSpace(providerId), OutboundEffectDomainErrors.ProviderIdRequired);
        Guard.Against(string.IsNullOrWhiteSpace(effectType), OutboundEffectDomainErrors.EffectTypeRequired);
        Guard.Against(string.IsNullOrWhiteSpace(idempotencyKey), OutboundEffectDomainErrors.IdempotencyKeyRequired);
        Guard.Against(string.IsNullOrWhiteSpace(schedulerActorId), OutboundEffectDomainErrors.SchedulerActorIdRequired);

        var aggregate = new OutboundEffectAggregate { Id = new OutboundEffectId(effectId) };
        aggregate.RaiseDomainEvent(new OutboundEffectScheduledEvent(
            new AggregateId(effectId),
            providerId,
            effectType,
            idempotencyKey,
            payloadTypeDiscriminator,
            schedulerActorId,
            dispatchTimeoutMs,
            totalBudgetMs,
            ackTimeoutMs,
            finalityWindowMs,
            maxAttempts,
            payload));
        return aggregate;
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case OutboundEffectScheduledEvent e:
                Id = new OutboundEffectId(ResolveAggregateId(e.AggregateId));
                _providerId = e.ProviderId;
                _effectType = e.EffectType;
                _idempotencyKey = e.IdempotencyKey;
                _payloadTypeDiscriminator = e.PayloadTypeDiscriminator;
                _maxAttempts = e.MaxAttempts;
                _dispatchTimeoutMs = e.DispatchTimeoutMs;
                _totalBudgetMs = e.TotalBudgetMs;
                _ackTimeoutMs = e.AckTimeoutMs;
                _finalityWindowMs = e.FinalityWindowMs;
                _attemptCount = 0;
                _status = OutboundEffectStatus.Scheduled;
                break;

            case OutboundEffectDispatchedEvent e:
                Guard.Against(
                    _status != OutboundEffectStatus.Scheduled && _status != OutboundEffectStatus.TransientFailed,
                    OutboundEffectDomainErrors.CannotDispatchUnlessScheduled);
                _attemptCount = e.AttemptNumber;
                _status = OutboundEffectStatus.Dispatched;
                break;

            case OutboundEffectAcknowledgedEvent e:
                Guard.Against(
                    _status != OutboundEffectStatus.Dispatched,
                    OutboundEffectDomainErrors.CannotAcknowledgeUnlessDispatched);
                Guard.Against(
                    string.IsNullOrWhiteSpace(e.ProviderId) || string.IsNullOrWhiteSpace(e.ProviderOperationId),
                    OutboundEffectDomainErrors.AcknowledgedRequiresProviderOperationId);
                _providerOperationId = e.ProviderOperationId;
                _status = OutboundEffectStatus.Acknowledged;
                break;

            case OutboundEffectDispatchFailedEvent e:
                _attemptCount = e.AttemptNumber;
                _failureClassification = e.Classification;
                _failureReason = e.Reason;
                _status = e.Classification == "Terminal"
                    ? OutboundEffectStatus.RetryExhausted
                    : OutboundEffectStatus.TransientFailed;
                break;

            case OutboundEffectRetryAttemptedEvent e:
                Guard.Against(
                    _status == OutboundEffectStatus.RetryExhausted,
                    OutboundEffectDomainErrors.CannotRetryAfterExhausted);
                _attemptCount = e.AttemptNumber;
                _status = OutboundEffectStatus.Scheduled;
                break;

            case OutboundEffectRetryExhaustedEvent e:
                _attemptCount = e.TotalAttempts;
                _failureClassification = e.FinalClassification;
                _status = OutboundEffectStatus.RetryExhausted;
                break;

            case OutboundEffectFinalizedEvent e:
                Guard.Against(
                    IsTerminal(_status) && _status != OutboundEffectStatus.Acknowledged && _status != OutboundEffectStatus.Dispatched,
                    OutboundEffectDomainErrors.CannotFinalizeFromTerminal);
                _finalityOutcome = e.FinalityOutcome;
                _status = OutboundEffectStatus.Finalized;
                break;

            case OutboundEffectReconciliationRequiredEvent e:
                _reconciliationCause = e.Cause;
                _status = OutboundEffectStatus.ReconciliationRequired;
                break;

            case OutboundEffectReconciledEvent e:
                Guard.Against(
                    _status != OutboundEffectStatus.ReconciliationRequired,
                    OutboundEffectDomainErrors.CannotReconcileUnlessRequested);
                _finalityOutcome = e.Outcome;
                _status = OutboundEffectStatus.Reconciled;
                break;

            case OutboundEffectCompensationRequestedEvent:
                _status = OutboundEffectStatus.CompensationRequested;
                break;

            case OutboundEffectCancelledEvent:
                Guard.Against(
                    _status != OutboundEffectStatus.Scheduled,
                    OutboundEffectDomainErrors.CannotCancelUnlessScheduled);
                _status = OutboundEffectStatus.Cancelled;
                break;
        }
    }

    protected override void EnsureInvariants() { }

    private Guid ResolveAggregateId(AggregateId aggregateId)
    {
        if (aggregateId.Value != Guid.Empty) return aggregateId.Value;
        return AggregateIdentity != Guid.Empty ? AggregateIdentity : Id.Value;
    }

    private static bool IsTerminal(OutboundEffectStatus status) =>
        status is OutboundEffectStatus.Finalized
            or OutboundEffectStatus.Cancelled
            or OutboundEffectStatus.Reconciled
            or OutboundEffectStatus.RetryExhausted;
}
