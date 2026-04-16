using Whycespace.Domain.EconomicSystem.Revenue.Contract;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Revenue.Contract;

public sealed class RevenueContractTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp Now = new(new DateTimeOffset(2026, 4, 7, 12, 0, 0, TimeSpan.Zero));
    private static readonly ContractTerm ValidTerm = new(
        new Timestamp(Now.Value),
        new Timestamp(Now.Value.AddYears(1)));

    private static IReadOnlyList<RevenueShareRule> TwoPartyShares(string seed) =>
        new[]
        {
            new RevenueShareRule(IdGen.Generate($"RevenueContractTests:{seed}:partyA"), 60m),
            new RevenueShareRule(IdGen.Generate($"RevenueContractTests:{seed}:partyB"), 40m),
        };

    [Fact]
    public void CreateContract_WithValidShares_RaisesContractCreatedEvent()
    {
        var contractId = RevenueContractId.From(IdGen.Generate("RevenueContractTests:Create:contract"));
        var shares = TwoPartyShares("Create");

        var aggregate = RevenueContractAggregate.CreateContract(contractId, shares, ValidTerm, Now);

        Assert.Equal(ContractStatus.Draft, aggregate.Status);
        Assert.Equal(2, aggregate.Parties.Count);

        var evt = Assert.IsType<RevenueContractCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(contractId, evt.ContractId);
        Assert.Equal(2, evt.RevenueShareRules.Count);
    }

    [Fact]
    public void CreateContract_WhenSharesDoNotTotal100_Throws()
    {
        var contractId = RevenueContractId.From(IdGen.Generate("RevenueContractTests:BadShares:contract"));
        var badShares = new[]
        {
            new RevenueShareRule(IdGen.Generate("RevenueContractTests:BadShares:a"), 70m),
            new RevenueShareRule(IdGen.Generate("RevenueContractTests:BadShares:b"), 20m),
        };

        Assert.Throws<DomainInvariantViolationException>(() =>
            RevenueContractAggregate.CreateContract(contractId, badShares, ValidTerm, Now));
    }

    [Fact]
    public void FullLifecycle_Create_Activate_Terminate_EmitsEventsInOrder()
    {
        var contractId = RevenueContractId.From(IdGen.Generate("RevenueContractTests:Lifecycle:contract"));

        var aggregate = RevenueContractAggregate.CreateContract(
            contractId, TwoPartyShares("Lifecycle"), ValidTerm, Now);

        aggregate.Activate(new Timestamp(Now.Value.AddHours(1)));
        Assert.Equal(ContractStatus.Active, aggregate.Status);

        aggregate.Terminate("partner default", new Timestamp(Now.Value.AddDays(30)));
        Assert.Equal(ContractStatus.Terminated, aggregate.Status);

        Assert.Equal(3, aggregate.DomainEvents.Count);
        Assert.IsType<RevenueContractCreatedEvent>(aggregate.DomainEvents[0]);
        Assert.IsType<RevenueContractActivatedEvent>(aggregate.DomainEvents[1]);

        var terminated = Assert.IsType<RevenueContractTerminatedEvent>(aggregate.DomainEvents[2]);
        Assert.Equal("partner default", terminated.Reason);

        Assert.Throws<DomainException>(() =>
            aggregate.Terminate("again", new Timestamp(Now.Value.AddDays(31))));
    }

    [Fact]
    public void LoadFromHistory_AfterFullLifecycle_ReconstitutesFinalState()
    {
        var contractId = RevenueContractId.From(IdGen.Generate("RevenueContractTests:History:contract"));
        var shares = TwoPartyShares("History");

        var history = new object[]
        {
            new RevenueContractCreatedEvent(contractId, shares, ValidTerm, Now),
            new RevenueContractActivatedEvent(contractId, new Timestamp(Now.Value.AddHours(1))),
            new RevenueContractTerminatedEvent(contractId, "end of term",
                new Timestamp(Now.Value.AddYears(1))),
        };

        var aggregate = (RevenueContractAggregate)Activator.CreateInstance(
            typeof(RevenueContractAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(contractId, aggregate.ContractId);
        Assert.Equal(ContractStatus.Terminated, aggregate.Status);
        Assert.Equal(2, aggregate.Parties.Count);
        Assert.Equal(2, aggregate.Version);
        Assert.Empty(aggregate.DomainEvents);
    }
}
