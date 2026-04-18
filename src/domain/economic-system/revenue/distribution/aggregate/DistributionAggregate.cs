using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Distribution;

/// <summary>
/// Distribution of SPV revenue across participant shares. Shares are
/// computed from CapitalAllocationAggregate ownership percentages supplied
/// by the caller — this aggregate does not mutate the vault. Invariant:
/// Sum(Percentage) across shares must equal 100.
///
/// State machine:
///   Created -> Confirmed -> Paid -> CompensationRequested -> Compensated
///   Created -> Confirmed -> Failed -> CompensationRequested -> Compensated
/// Compensated is terminal and irreversible (Phase 7 T7.2).
/// </summary>
public sealed class DistributionAggregate : AggregateRoot
{
    private readonly List<ParticipantShare> _shares = new();

    public DistributionId DistributionId { get; private set; }
    public string SpvId { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public DistributionStatus Status { get; private set; }
    public IReadOnlyList<ParticipantShare> Shares => _shares.AsReadOnly();

    private DistributionAggregate() { }

    public static DistributionAggregate CreateDistribution(
        DistributionId distributionId,
        string spvId,
        decimal totalAmount,
        IReadOnlyList<(string ParticipantId, decimal OwnershipPercentage)> allocations)
    {
        if (string.IsNullOrWhiteSpace(spvId))
            throw new ArgumentException("SpvId cannot be empty.", nameof(spvId));

        if (totalAmount <= 0m)
            throw new ArgumentException("TotalAmount must be greater than zero.", nameof(totalAmount));

        if (allocations is null || allocations.Count == 0)
            throw new ArgumentException("Distribution requires at least one allocation.", nameof(allocations));

        var computedShares = new List<ParticipantShare>(allocations.Count);
        decimal percentageSum = 0m;

        foreach (var (participantId, ownershipPct) in allocations)
        {
            var shareAmount = totalAmount * (ownershipPct / 100m);
            computedShares.Add(new ParticipantShare(participantId, shareAmount, ownershipPct));
            percentageSum += ownershipPct;
        }

        if (percentageSum != 100m)
            throw new ArgumentException(
                $"Sum of allocation percentages must equal 100 (was {percentageSum}).",
                nameof(allocations));

        var aggregate = new DistributionAggregate();

        aggregate.RaiseDomainEvent(new DistributionCreatedEvent(
            distributionId.Value.ToString(),
            spvId,
            totalAmount));

        foreach (var share in computedShares)
        {
            aggregate._shares.Add(share);
        }

        return aggregate;
    }

    public void Confirm(Timestamp confirmedAt)
    {
        if (Status == DistributionStatus.Confirmed)
            throw DistributionErrors.AlreadyConfirmed();
        if (Status != DistributionStatus.Created)
            throw DistributionErrors.InvalidTransition(Status, DistributionStatus.Confirmed);

        RaiseDomainEvent(new DistributionConfirmedEvent(
            DistributionId.Value.ToString(),
            confirmedAt));
    }

    public void MarkPaid(string payoutId, Timestamp paidAt)
    {
        if (string.IsNullOrWhiteSpace(payoutId))
            throw DistributionErrors.PayoutIdRequired();
        if (Status != DistributionStatus.Confirmed)
            throw DistributionErrors.InvalidTransition(Status, DistributionStatus.Paid);

        RaiseDomainEvent(new DistributionPaidEvent(
            DistributionId.Value.ToString(),
            payoutId,
            paidAt));
    }

    public void MarkFailed(string reason, Timestamp failedAt)
    {
        if (Status == DistributionStatus.Paid || Status == DistributionStatus.Failed)
            throw DistributionErrors.AlreadyTerminal();

        RaiseDomainEvent(new DistributionFailedEvent(
            DistributionId.Value.ToString(),
            reason ?? string.Empty,
            failedAt));
    }

    public void RequestCompensation(string originalPayoutId, string reason, Timestamp requestedAt)
    {
        if (string.IsNullOrWhiteSpace(originalPayoutId))
            throw DistributionErrors.CompensationCorrelationRequired();

        if (Status == DistributionStatus.Compensated || Status == DistributionStatus.CompensationRequested)
            throw DistributionErrors.AlreadyCompensated();

        if (Status != DistributionStatus.Paid && Status != DistributionStatus.Failed)
            throw DistributionErrors.CompensationNotAllowed(Status);

        RaiseDomainEvent(new DistributionCompensationRequestedEvent(
            DistributionId.Value.ToString(),
            originalPayoutId,
            reason ?? string.Empty,
            requestedAt));
    }

    public void MarkCompensated(string originalPayoutId, string compensatingJournalId, Timestamp compensatedAt)
    {
        if (string.IsNullOrWhiteSpace(originalPayoutId))
            throw DistributionErrors.CompensationCorrelationRequired();
        if (string.IsNullOrWhiteSpace(compensatingJournalId))
            throw DistributionErrors.CompensatingJournalIdRequired();

        if (Status == DistributionStatus.Compensated)
            throw DistributionErrors.AlreadyCompensated();
        if (Status != DistributionStatus.CompensationRequested)
            throw DistributionErrors.CompensationNotRequested();

        RaiseDomainEvent(new DistributionCompensatedEvent(
            DistributionId.Value.ToString(),
            originalPayoutId,
            compensatingJournalId,
            compensatedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case DistributionCreatedEvent e:
                DistributionId = DistributionId.From(Guid.Parse(e.DistributionId));
                SpvId = e.SpvId;
                TotalAmount = e.TotalAmount;
                Status = DistributionStatus.Created;
                break;
            case DistributionConfirmedEvent:
                Status = DistributionStatus.Confirmed;
                break;
            case DistributionPaidEvent:
                Status = DistributionStatus.Paid;
                break;
            case DistributionFailedEvent:
                Status = DistributionStatus.Failed;
                break;
            case DistributionCompensationRequestedEvent:
                Status = DistributionStatus.CompensationRequested;
                break;
            case DistributionCompensatedEvent:
                Status = DistributionStatus.Compensated;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (_shares.Count == 0)
            return;

        decimal percentageSum = 0m;
        foreach (var share in _shares)
            percentageSum += share.Percentage;

        if (percentageSum != 100m)
            throw new DomainInvariantViolationException(
                $"Invariant violated: ParticipantShare percentages must sum to 100 (was {percentageSum}).");
    }
}
