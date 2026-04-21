using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Command.CommandEnvelope;

public readonly record struct CommandEnvelopeId
{
    public Guid Value { get; }

    public CommandEnvelopeId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "CommandEnvelopeId cannot be empty.");
        Value = value;
    }
}
