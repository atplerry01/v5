using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Domain.StructuralSystem.Humancapital.Participant;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Humancapital.Participant;

namespace Whycespace.Engines.T2E.Structural.Humancapital.Participant;

public sealed class PlaceParticipantHandler : IEngine
{
    private readonly IStructuralParentLookup _parentLookup;

    public PlaceParticipantHandler(IStructuralParentLookup parentLookup)
    {
        _parentLookup = parentLookup;
    }

    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not PlaceParticipantCommand cmd) return Task.CompletedTask;
        var aggregate = ParticipantAggregate.Place(
            new ParticipantId(cmd.ParticipantId.ToString()),
            new ClusterRef(cmd.HomeClusterId),
            cmd.EffectiveAt,
            _parentLookup);
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
