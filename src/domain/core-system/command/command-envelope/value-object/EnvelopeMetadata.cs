using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Command.CommandEnvelope;

public readonly record struct EnvelopeMetadata
{
    public Guid CorrelationId { get; }
    public Guid CausationId { get; }
    public string CommandName { get; }

    public EnvelopeMetadata(Guid correlationId, Guid causationId, string commandName)
    {
        Guard.Against(correlationId == Guid.Empty, "CorrelationId must not be empty.");
        Guard.Against(causationId == Guid.Empty, "CausationId must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(commandName), "CommandName must not be empty.");

        CorrelationId = correlationId;
        CausationId = causationId;
        CommandName = commandName;
    }
}
