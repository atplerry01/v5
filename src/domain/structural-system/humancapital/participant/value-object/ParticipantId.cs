using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Participant;

public sealed record ParticipantId
{
    public string Value { get; }

    public ParticipantId(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "ParticipantId cannot be empty.");
        Value = value;
    }
}
