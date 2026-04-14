using Whycespace.Domain.EconomicSystem.Revenue.Distribution;
using Whycespace.Shared.Contracts.Economic.Revenue.Distribution;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Revenue.Distribution;

public sealed class CreateDistributionHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateDistributionCommand cmd)
            return Task.CompletedTask;

        var allocations = new List<(string ParticipantId, decimal OwnershipPercentage)>(cmd.Allocations.Count);
        foreach (var a in cmd.Allocations)
            allocations.Add((a.ParticipantId, a.OwnershipPercentage));

        var aggregate = DistributionAggregate.CreateDistribution(
            new DistributionId(cmd.DistributionId),
            cmd.SpvId,
            cmd.TotalAmount,
            allocations);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
