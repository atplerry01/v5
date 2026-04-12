using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Contract;

public sealed class RevenueContractAggregate : AggregateRoot
{
    private readonly List<ContractParty> _parties = new();

    public RevenueContractId ContractId { get; private set; }
    public IReadOnlyList<ContractParty> Parties => _parties.AsReadOnly();
    public ContractTerm Term { get; private set; }
    public ContractStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }

    private RevenueContractAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static RevenueContractAggregate CreateContract(
        RevenueContractId contractId,
        IReadOnlyList<RevenueShareRule> revenueShareRules,
        ContractTerm term,
        Timestamp createdAt)
    {
        if (revenueShareRules.Count < 2) throw ContractErrors.InsufficientParties();
        if (term.EndDate.Value <= term.StartDate.Value) throw ContractErrors.InvalidTerm();

        foreach (var rule in revenueShareRules)
        {
            if (rule.PartyId == Guid.Empty) throw ContractErrors.InvalidPartyId();
            if (rule.SharePercentage <= 0m || rule.SharePercentage > 100m)
                throw ContractErrors.InvalidSharePercentage();
        }

        var totalShare = 0m;
        foreach (var rule in revenueShareRules)
            totalShare += rule.SharePercentage;

        if (totalShare != 100m)
            throw ContractErrors.SharesMustTotal100(totalShare);

        var aggregate = new RevenueContractAggregate();
        aggregate.RaiseDomainEvent(new RevenueContractCreatedEvent(
            contractId, revenueShareRules, term, createdAt));
        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void Activate(Timestamp activatedAt)
    {
        if (Status == ContractStatus.Active) throw ContractErrors.ContractAlreadyActive();
        if (Status == ContractStatus.Terminated) throw ContractErrors.ContractAlreadyTerminated();
        if (Status != ContractStatus.Draft) throw ContractErrors.ContractNotDraft();

        RaiseDomainEvent(new RevenueContractActivatedEvent(ContractId, activatedAt));
    }

    public void Terminate(string reason, Timestamp terminatedAt)
    {
        if (Status == ContractStatus.Terminated) throw ContractErrors.ContractAlreadyTerminated();

        RaiseDomainEvent(new RevenueContractTerminatedEvent(ContractId, reason, terminatedAt));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case RevenueContractCreatedEvent e:
                ContractId = e.ContractId;
                Term = e.Term;
                Status = ContractStatus.Draft;
                CreatedAt = e.CreatedAt;
                foreach (var rule in e.RevenueShareRules)
                    _parties.Add(ContractParty.Create(rule.PartyId, rule.SharePercentage));
                break;

            case RevenueContractActivatedEvent:
                Status = ContractStatus.Active;
                break;

            case RevenueContractTerminatedEvent:
                Status = ContractStatus.Terminated;
                break;
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (_parties.Count < 2)
            throw ContractErrors.InsufficientPartiesInvariant(_parties.Count);

        var totalShare = 0m;
        foreach (var party in _parties)
            totalShare += party.SharePercentage;

        if (totalShare != 100m)
            throw ContractErrors.SharesMustTotal100(totalShare);
    }
}
