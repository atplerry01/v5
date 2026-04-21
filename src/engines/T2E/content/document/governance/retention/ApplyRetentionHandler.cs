using Whycespace.Domain.ContentSystem.Document.Governance.Retention;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Document.Governance.Retention;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Document.Governance.Retention;

public sealed class ApplyRetentionHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ApplyRetentionCommand cmd)
            return Task.CompletedTask;

        var aggregate = RetentionAggregate.Apply(
            new RetentionId(cmd.RetentionId),
            new RetentionTargetRef(cmd.TargetId, Enum.Parse<RetentionTargetKind>(cmd.TargetKind)),
            new RetentionWindow(
                new Timestamp(cmd.WindowAppliedAt),
                new Timestamp(cmd.WindowExpiresAt)),
            new RetentionReason(cmd.Reason),
            new Timestamp(cmd.AppliedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
