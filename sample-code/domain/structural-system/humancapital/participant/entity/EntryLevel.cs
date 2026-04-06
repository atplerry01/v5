namespace Whycespace.Domain.StructuralSystem.HumanCapital.Participant;

public sealed class EntryLevel
{
    public Guid Id { get; }
    public string Level { get; }

    public EntryLevel(Guid id, string level)
    {
        Id = id;
        Level = level;
    }
}
