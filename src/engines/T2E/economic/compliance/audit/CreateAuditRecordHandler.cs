using Whycespace.Domain.DecisionSystem.Compliance.Audit;
using Whycespace.Domain.SharedKernel.Primitive.Identity;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Compliance.Audit;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Compliance.Audit;

public sealed class CreateAuditRecordHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateAuditRecordCommand cmd)
            return Task.CompletedTask;

        if (!Enum.TryParse<AuditType>(cmd.AuditType, ignoreCase: true, out var auditType))
            throw new InvalidOperationException($"Unknown audit type: '{cmd.AuditType}'.");

        if (!ContentId.TryParse(cmd.EvidenceSummary, out var evidenceContentId))
            throw new InvalidOperationException(
                "CreateAuditRecordCommand.EvidenceSummary must be a non-empty GUID identifying the evidence content aggregate (ContentId).");

        var aggregate = AuditRecordAggregate.CreateRecord(
            new AuditRecordId(cmd.AuditRecordId),
            new SourceDomain(cmd.SourceDomain),
            new SourceAggregateId(cmd.SourceAggregateId),
            new SourceEventId(cmd.SourceEventId),
            auditType,
            new DocumentRef(evidenceContentId),
            new Timestamp(cmd.RecordedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
