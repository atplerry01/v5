using Whycespace.Domain.EconomicSystem.Revenue.Distribution;
using Whycespace.Domain.EconomicSystem.Revenue.Payout;
using Whycespace.Shared.Contracts.Economic.Revenue.Payout;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Revenue.Payout;

public sealed class ExecutePayoutHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ExecutePayoutCommand cmd)
            return Task.CompletedTask;

        var shares = new List<ParticipantShare>(cmd.Shares.Count);
        foreach (var s in cmd.Shares)
            shares.Add(new ParticipantShare(s.ParticipantId, s.Amount, s.Percentage));

        var aggregate = PayoutAggregate.ExecutePayout(
            cmd.PayoutId.ToString(),
            cmd.DistributionId.ToString(),
            shares);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
