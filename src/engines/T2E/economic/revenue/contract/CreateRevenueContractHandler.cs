using Whycespace.Domain.EconomicSystem.Revenue.Contract;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Revenue.Contract;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T2E.Economic.Revenue.Contract;

public sealed class CreateRevenueContractHandler : IEngine
{
    private readonly IClock _clock;

    public CreateRevenueContractHandler(IClock clock) => _clock = clock;

    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateRevenueContractCommand cmd)
            return Task.CompletedTask;

        var rules = new List<RevenueShareRule>(cmd.ShareRules.Count);
        foreach (var r in cmd.ShareRules)
            rules.Add(new RevenueShareRule(r.PartyId, r.SharePercentage));

        var aggregate = RevenueContractAggregate.CreateContract(
            new RevenueContractId(cmd.ContractId),
            rules,
            new ContractTerm(
                new Timestamp(cmd.TermStart),
                new Timestamp(cmd.TermEnd)),
            new Timestamp(_clock.UtcNow));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
