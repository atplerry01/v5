using Whycespace.Domain.ConstitutionalSystem.Chain.EvidenceRecord;
using Whycespace.Shared.Contracts.Constitutional.Chain.EvidenceRecord;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Constitutional.Chain.EvidenceRecord;

public sealed class RecordEvidenceHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RecordEvidenceCommand cmd)
            return Task.CompletedTask;

        if (!Enum.TryParse<Domain.ConstitutionalSystem.Chain.EvidenceRecord.EvidenceType>(
                cmd.EvidenceType, ignoreCase: true, out var evidenceType))
            evidenceType = Domain.ConstitutionalSystem.Chain.EvidenceRecord.EvidenceType.Event;

        var aggregate = EvidenceRecordAggregate.Record(
            new Domain.ConstitutionalSystem.Chain.EvidenceRecord.EvidenceRecordId(cmd.EvidenceRecordId),
            new EvidenceDescriptor(
                cmd.CorrelationId,
                cmd.AnchorRecordId,
                evidenceType,
                cmd.ActorId,
                cmd.SubjectId,
                cmd.PolicyHash),
            cmd.RecordedAt);

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
