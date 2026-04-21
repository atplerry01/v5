using Whycespace.Domain.StructuralSystem.Humancapital.Participant;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Humancapital.Participant;

namespace Whycespace.Engines.T2E.Structural.Humancapital.Participant;

public sealed class RegisterParticipantHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RegisterParticipantCommand cmd) return Task.CompletedTask;
        var aggregate = ParticipantAggregate.Register(
            new ParticipantId(cmd.ParticipantId.ToString()));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
