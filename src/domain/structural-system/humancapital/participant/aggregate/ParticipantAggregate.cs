namespace Whycespace.Domain.StructuralSystem.Humancapital.Participant;

public sealed class ParticipantAggregate
{
    public ParticipantId Id { get; private set; } = null!;

    private ParticipantAggregate() { }

    public static ParticipantAggregate Register(ParticipantId id)
    {
        var agg = new ParticipantAggregate();
        agg.Apply(new ParticipantRegisteredEvent(id.Value));
        return agg;
    }

    private void Apply(ParticipantRegisteredEvent e)
    {
        Id = new ParticipantId(e.ParticipantId);
    }
}
