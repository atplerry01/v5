using Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Eligibility;
using Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Eligibility;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Entitlement.EligibilityAndGrant.Eligibility;

public sealed class CreateEligibilityHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateEligibilityCommand cmd)
            return Task.CompletedTask;

        var aggregate = EligibilityAggregate.Create(
            new EligibilityId(cmd.EligibilityId),
            new EligibilitySubjectRef(cmd.SubjectId),
            new EligibilityTargetRef(cmd.TargetId),
            new EligibilityScope(cmd.Scope));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
