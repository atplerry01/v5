using Whycespace.Domain.ConstitutionalSystem.Chain.AnchorRecord;
using Whycespace.Shared.Contracts.Constitutional.Chain.AnchorRecord;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Constitutional.Chain.AnchorRecord;

public sealed class RecordAnchorHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RecordAnchorCommand cmd)
            return Task.CompletedTask;

        var aggregate = AnchorRecordAggregate.Record(
            new Domain.ConstitutionalSystem.Chain.AnchorRecord.AnchorRecordId(cmd.AnchorRecordId),
            new AnchorDescriptor(
                cmd.CorrelationId,
                cmd.BlockHash,
                cmd.EventHash,
                cmd.PreviousBlockHash,
                cmd.DecisionHash,
                cmd.Sequence),
            cmd.AnchoredAt);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
