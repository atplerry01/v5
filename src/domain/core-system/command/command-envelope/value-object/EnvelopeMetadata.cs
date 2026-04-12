namespace Whycespace.Domain.CoreSystem.Command.CommandEnvelope;

public readonly record struct EnvelopeMetadata
{
    public Guid CorrelationId { get; }
    public Guid CausationId { get; }
    public string CommandName { get; }

    public EnvelopeMetadata(Guid correlationId, Guid causationId, string commandName)
    {
        if (correlationId == Guid.Empty)
            throw new ArgumentException("CorrelationId must not be empty.", nameof(correlationId));

        if (causationId == Guid.Empty)
            throw new ArgumentException("CausationId must not be empty.", nameof(causationId));

        if (string.IsNullOrWhiteSpace(commandName))
            throw new ArgumentException("CommandName must not be empty.", nameof(commandName));

        CorrelationId = correlationId;
        CausationId = causationId;
        CommandName = commandName;
    }
}
