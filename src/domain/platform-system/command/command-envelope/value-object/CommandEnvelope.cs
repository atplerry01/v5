using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Command.CommandEnvelope;

public sealed record CommandEnvelope
{
    public CommandId CommandId { get; }
    public CommandType Type { get; }
    public DomainRoute Source { get; }
    public DomainRoute Destination { get; }
    public Guid CorrelationId { get; }
    public Guid CausationId { get; }
    public Timestamp IssuedAt { get; }
    public ReadOnlyMemory<byte> Payload { get; }

    public CommandEnvelope(
        CommandId commandId,
        CommandType type,
        DomainRoute source,
        DomainRoute destination,
        Guid correlationId,
        Guid causationId,
        Timestamp issuedAt,
        ReadOnlyMemory<byte> payload)
    {
        Guard.Against(!source.IsValid(), "CommandEnvelope source must be a valid DomainRoute.");
        Guard.Against(!destination.IsValid(), "CommandEnvelope destination must be a valid DomainRoute.");
        Guard.Against(correlationId == Guid.Empty, "CommandEnvelope correlationId cannot be empty.");
        Guard.Against(causationId == Guid.Empty, "CommandEnvelope causationId cannot be empty.");

        CommandId = commandId;
        Type = type;
        Source = source;
        Destination = destination;
        CorrelationId = correlationId;
        CausationId = causationId;
        IssuedAt = issuedAt;
        Payload = payload;
    }
}
