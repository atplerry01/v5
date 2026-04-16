using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Distribution;

/// <summary>
/// Distribution of SPV revenue across participant shares. Shares are
/// computed from CapitalAllocationAggregate ownership percentages supplied
/// by the caller — this aggregate does not mutate the vault. Invariant:
/// Sum(Percentage) across shares must equal 100.
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

    /// <summary>
    /// Creates a distribution. Each allocation pair is (participantId, ownershipPercentage);
    /// the share amount is computed as totalAmount * (ownershipPercentage / 100).
    /// </summary>
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
