namespace Whycespace.Domain.StructuralSystem.HumanCapital.Participant;

public sealed class ParticipantType
{
    public Guid Id { get; }
    public string Name { get; }

    public ParticipantType(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}
