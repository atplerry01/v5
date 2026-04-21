using Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.UsageRight;
using Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.UsageRight;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Entitlement.UsageControl.UsageRight;

public sealed class CreateUsageRightHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateUsageRightCommand cmd)
            return Task.CompletedTask;

        var aggregate = UsageRightAggregate.Create(
            new UsageRightId(cmd.UsageRightId),
            new UsageRightSubjectId(cmd.SubjectId),
            new UsageRightReferenceId(cmd.ReferenceId),
            cmd.TotalUnits);
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
