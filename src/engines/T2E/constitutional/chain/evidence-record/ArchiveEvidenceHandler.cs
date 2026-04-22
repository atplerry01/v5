using Whycespace.Domain.ConstitutionalSystem.Chain.EvidenceRecord;
using Whycespace.Shared.Contracts.Constitutional.Chain.EvidenceRecord;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Constitutional.Chain.EvidenceRecord;

public sealed class ArchiveEvidenceHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ArchiveEvidenceCommand cmd)
            return;

        var aggregate = (EvidenceRecordAggregate)await context.LoadAggregateAsync(typeof(EvidenceRecordAggregate));
        aggregate.Archive(cmd.ArchivedAt);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
