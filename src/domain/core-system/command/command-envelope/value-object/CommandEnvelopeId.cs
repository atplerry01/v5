namespace Whycespace.Domain.CoreSystem.Command.CommandEnvelope;

public readonly record struct CommandEnvelopeId
{
    public Guid Value { get; }

    public CommandEnvelopeId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CommandEnvelopeId cannot be empty.", nameof(value));

        Value = value;
    }
}
