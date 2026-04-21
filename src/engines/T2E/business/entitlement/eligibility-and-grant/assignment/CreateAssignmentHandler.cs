using Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Assignment;
using Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Assignment;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Entitlement.EligibilityAndGrant.Assignment;

public sealed class CreateAssignmentHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateAssignmentCommand cmd)
            return Task.CompletedTask;

        var aggregate = AssignmentAggregate.Create(
            new AssignmentId(cmd.AssignmentId),
            new GrantRef(cmd.GrantId),
            new AssignmentSubjectRef(cmd.SubjectId),
            new AssignmentScope(cmd.Scope));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
