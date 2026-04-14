namespace Whycespace.Domain.StructuralSystem.Humancapital.Participant;

public sealed record ParticipantId(string Value)
{
    public string Value { get; init; } = string.IsNullOrWhiteSpace(Value)
        ? throw new ArgumentException("ParticipantId cannot be empty", nameof(Value))
        : Value;
}
