using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Domain.StructuralSystem.Humancapital.Assignment;
using Whycespace.Domain.StructuralSystem.Humancapital.Participant;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Humancapital.Assignment;

namespace Whycespace.Engines.T2E.Structural.Humancapital.Assignment;

public sealed class AssignAssignmentHandler : IEngine
{
    private readonly IStructuralParentLookup _parentLookup;

    public AssignAssignmentHandler(IStructuralParentLookup parentLookup)
    {
        _parentLookup = parentLookup;
    }

    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not AssignAssignmentCommand cmd) return Task.CompletedTask;
        var aggregate = AssignmentAggregate.Assign(
            new AssignmentId(cmd.AssignmentId),
            new ParticipantId(cmd.ParticipantId.ToString()),
            new ClusterAuthorityRef(cmd.AuthorityId),
            cmd.EffectiveAt,
            _parentLookup);
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
