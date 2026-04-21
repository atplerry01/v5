using Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.Limit;
using Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.Limit;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Entitlement.UsageControl.Limit;

public sealed class CreateLimitHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateLimitCommand cmd)
            return Task.CompletedTask;

        var aggregate = LimitAggregate.Create(
            new LimitId(cmd.LimitId),
            new LimitSubjectId(cmd.SubjectId),
            cmd.ThresholdValue);
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
