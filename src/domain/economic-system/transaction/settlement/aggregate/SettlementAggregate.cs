using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Settlement;

/// <summary>
/// Settlement aggregate. Models the external-execution boundary for transaction
/// value movement (bank rails, payment providers). The domain owns the settlement
/// *reference* and *status* — it never calls external APIs, never persists, and
/// never mutates once a terminal state (Completed / Failed) is reached.
/// Lifecycle: Initiated -> Processing -> Completed (terminal)
///            Initiated -> Processing -> Failed    (terminal)
/// Runtime is responsible for persistence, Kafka publication, projection, and
/// WhyceChain anchoring. External rail integration is an adapter/worker concern
/// outside the domain.
/// </summary>
public sealed class SettlementAggregate : AggregateRoot
{
    public SettlementId SettlementId { get; private set; }
    public SettlementAmount Amount { get; private set; }
    public SettlementCurrency Currency { get; private set; }
    public SettlementSourceReference SourceReference { get; private set; }
    public SettlementProvider Provider { get; private set; }
    public SettlementStatus Status { get; private set; }
    public SettlementReference? Reference { get; private set; }
    public string FailureReason { get; private set; } = string.Empty;

    private SettlementAggregate() { }

    public static SettlementAggregate Initiate(
        SettlementId settlementId,
        SettlementAmount amount,
        SettlementCurrency currency,
        SettlementSourceReference sourceReference,
        SettlementProvider provider)
    {
        if (string.IsNullOrWhiteSpace(sourceReference.Value))
            throw SettlementErrors.MissingSourceReference();
        if (string.IsNullOrWhiteSpace(provider.Value))
            throw new ArgumentException("Provider must be set.", nameof(provider));

        var aggregate = new SettlementAggregate();

        aggregate.RaiseDomainEvent(new SettlementInitiatedEvent(
            settlementId.Value.ToString(),
            amount.Value,
            currency.Value,
            sourceReference.Value,
            provider.Value));

        return aggregate;
    }

    public void MarkProcessing()
    {
        GuardNotTerminal();

        var lifecycle = new SettlementLifecycleSpecification();
        if (!lifecycle.CanProcess(Status))
            throw SettlementErrors.InvalidStateTransition(Status, SettlementStatus.Processing);

        RaiseDomainEvent(new SettlementProcessingStartedEvent(SettlementId.Value.ToString()));
    }

    public void MarkCompleted(SettlementReferenceId externalReferenceId)
    {
        GuardNotTerminal();

        var lifecycle = new SettlementLifecycleSpecification();
        if (!lifecycle.CanComplete(Status))
            throw SettlementErrors.InvalidStateTransition(Status, SettlementStatus.Completed);

        if (string.IsNullOrWhiteSpace(externalReferenceId.Value))
            throw new ArgumentException("External reference is required to complete.", nameof(externalReferenceId));

        RaiseDomainEvent(new SettlementCompletedEvent(
            SettlementId.Value.ToString(),
            externalReferenceId.Value));
    }

    public void MarkFailed(string reason)
    {
        GuardNotTerminal();

        var lifecycle = new SettlementLifecycleSpecification();
        if (!lifecycle.CanFail(Status))
            throw SettlementErrors.InvalidStateTransition(Status, SettlementStatus.Failed);

        RaiseDomainEvent(new SettlementFailedEvent(
            SettlementId.Value.ToString(),
            reason ?? string.Empty));
    }

    private void GuardNotTerminal()
    {
        if (Status == SettlementStatus.Completed) throw SettlementErrors.AlreadyCompleted();
        if (Status == SettlementStatus.Failed) throw SettlementErrors.AlreadyFailed();
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case SettlementInitiatedEvent e:
                SettlementId = SettlementId.From(Guid.Parse(e.SettlementId));
                Amount = SettlementAmount.From(e.Amount);
                Currency = SettlementCurrency.From(e.Currency);
                SourceReference = SettlementSourceReference.From(e.SourceReference);
                Provider = SettlementProvider.From(e.Provider);
                Status = SettlementStatus.Initiated;
                break;

            case SettlementProcessingStartedEvent:
                Status = SettlementStatus.Processing;
                break;

            case SettlementCompletedEvent e:
                Status = SettlementStatus.Completed;
                Reference = SettlementReference.Create(
                    Provider,
                    SettlementReferenceId.From(e.ExternalReferenceId),
                    metadata: null);
                break;

            case SettlementFailedEvent e:
                Status = SettlementStatus.Failed;
                FailureReason = e.Reason ?? string.Empty;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Amount.Value < 0m)
            throw SettlementErrors.NegativeAmount();
    }
}
