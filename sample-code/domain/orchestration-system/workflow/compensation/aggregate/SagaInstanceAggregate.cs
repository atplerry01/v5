using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OrchestrationSystem.Workflow.Compensation;

public sealed class SagaInstanceAggregate : AggregateRoot
{
    private readonly List<SagaStepId> _completedSteps = [];
    private readonly List<CompensationAction> _compensationSteps = [];

    public SagaId SagaId { get; private set; } = default!;
    public Guid WorkflowInstanceId { get; private set; }
    public SagaType SagaType { get; private set; } = default!;
    public SagaState State { get; private set; }
    public SagaStepId? CurrentStep { get; private set; }
    public IReadOnlyList<SagaStepId> CompletedSteps => _completedSteps.AsReadOnly();
    public IReadOnlyList<CompensationAction> CompensationSteps => _compensationSteps.AsReadOnly();
    public CorrelationId CorrelationId { get; private set; } = CorrelationId.Empty;
    public CausationId CausationId { get; private set; } = CausationId.Empty;
    public TraceId TraceId { get; private set; } = TraceId.Empty;
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    private SagaInstanceAggregate() { }

    public static SagaInstanceAggregate Create(
        SagaId sagaId,
        Guid workflowInstanceId,
        SagaType sagaType,
        SagaStepId initialStep,
        CorrelationId correlationId,
        CausationId causationId = default,
        TraceId traceId = default)
    {
        Guard.AgainstNull(sagaId);
        Guard.AgainstNull(sagaType);
        Guard.AgainstNull(initialStep);
        Guard.AgainstDefault(workflowInstanceId);
        Guard.AgainstInvalid(correlationId, c => !c.IsEmpty, "CorrelationId must not be empty.");

        var resolvedTraceId = traceId.IsEmpty ? TraceId.FromSeed($"SagaTrace:{sagaId.Value}:{workflowInstanceId}") : traceId;

        var saga = new SagaInstanceAggregate();
        saga.ApplyEvent(new SagaStartedEvent(sagaId.Value, workflowInstanceId, sagaType.Value)
        {
            CorrelationId = correlationId,
            CausationId = causationId,
            TraceId = resolvedTraceId
        });
        saga.CurrentStep = initialStep;
        saga.CorrelationId = correlationId;
        saga.CausationId = causationId;
        saga.TraceId = resolvedTraceId;
        return saga;
    }

    public void Start()
    {
        EnsureInvariant(
            State == SagaState.Pending,
            "INVALID_STATE_TRANSITION",
            $"Cannot start saga in state '{State}'.");

        State = SagaState.Running;
    }

    public void AdvanceStep(SagaStepId nextStep)
    {
        Guard.AgainstNull(nextStep);

        EnsureInvariant(
            State == SagaState.Running,
            "INVALID_STATE_TRANSITION",
            $"Cannot advance saga in state '{State}'.");

        EnsureInvariant(
            CurrentStep is not null,
            "CONSISTENCY",
            "No current step to complete.");

        if (_completedSteps.Contains(CurrentStep!))
            return; // idempotent

        _completedSteps.Add(CurrentStep!);
        ApplyEvent(new SagaStepCompletedEvent(SagaId.Value, CurrentStep!.Value)
        {
            CorrelationId = CorrelationId,
            CausationId = CausationId,
            TraceId = TraceId
        });
        CurrentStep = nextStep;
    }

    public void MarkCompleted(DateTimeOffset timestamp)
    {
        var spec = new SagaCompletionSpecification();
        EnsureInvariant(
            spec.IsSatisfiedBy(this),
            "COMPLETION_CONDITIONS",
            "Saga completion conditions not met.");

        State = SagaState.Completed;
        CompletedAt = timestamp;
        ApplyEvent(new SagaCompletedEvent(SagaId.Value, _completedSteps.Count)
        {
            CorrelationId = CorrelationId,
            CausationId = CausationId,
            TraceId = TraceId
        });
    }

    public void MarkFailed(SagaStepId failedStep, string reason)
    {
        Guard.AgainstNull(failedStep);
        Guard.AgainstEmpty(reason);

        EnsureInvariant(
            State is not SagaState.Completed and not SagaState.Failed,
            "INVALID_STATE_TRANSITION",
            $"Cannot fail saga in state '{State}'.");

        State = SagaState.Failed;
        CurrentStep = null;
        ApplyEvent(new SagaFailedEvent(SagaId.Value, failedStep.Value, reason)
        {
            CorrelationId = CorrelationId,
            CausationId = CausationId,
            TraceId = TraceId
        });
    }

    public void TriggerCompensation(SagaStepId failedStep, IReadOnlyList<CompensationAction> compensations)
    {
        Guard.AgainstNull(failedStep);
        Guard.AgainstNull(compensations);

        EnsureInvariant(
            State == SagaState.Failed,
            "INVALID_STATE_TRANSITION",
            "Compensation can only be triggered from a failed state.");

        EnsureInvariant(
            compensations.Count > 0,
            "CONSISTENCY",
            "At least one compensation action is required.");

        State = SagaState.Compensating;
        _compensationSteps.AddRange(compensations);
        ApplyEvent(new SagaCompensationTriggeredEvent(SagaId.Value, failedStep.Value, compensations.Count)
        {
            CorrelationId = CorrelationId,
            CausationId = CausationId,
            TraceId = TraceId
        });
    }

    public void ApplyEvent(DomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case SagaStartedEvent e:
                SagaId = new SagaId(e.SagaId);
                WorkflowInstanceId = e.WorkflowInstanceId;
                SagaType = new SagaType(e.SagaType);
                State = SagaState.Pending;
                StartedAt = e.OccurredAt;
                CorrelationId = e.CorrelationId;
                CausationId = e.CausationId;
                TraceId = e.TraceId;
                break;

            case SagaStepCompletedEvent:
            case SagaCompletedEvent:
            case SagaFailedEvent:
            case SagaCompensationTriggeredEvent:
                break;
        }

        RaiseDomainEvent(domainEvent);
    }
}
