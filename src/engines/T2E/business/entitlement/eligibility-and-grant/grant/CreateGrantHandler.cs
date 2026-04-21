using Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Grant;
using Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Grant;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Entitlement.EligibilityAndGrant.Grant;

public sealed class CreateGrantHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateGrantCommand cmd)
            return Task.CompletedTask;

        GrantExpiry? expiry = cmd.ExpiresAt.HasValue
            ? new GrantExpiry(cmd.ExpiresAt.Value)
            : null;

        var aggregate = GrantAggregate.Create(
            new GrantId(cmd.GrantId),
            new GrantSubjectRef(cmd.SubjectId),
            new GrantTargetRef(cmd.TargetId),
            new GrantScope(cmd.Scope),
            cmd.CreatedAt,
            expiry);
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
