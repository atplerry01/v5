namespace Whycespace.Domain.ConstitutionalSystem.Chain.Ledger;

public sealed class ChainBlockAggregate : AggregateRoot
{
    public HashValue PreviousHash { get; private set; } = null!;
    public HashValue CurrentHash { get; private set; } = null!;
    public HashValue PayloadHash { get; private set; } = null!;
    public PayloadDescriptor Descriptor { get; private set; } = null!;
    public long SequenceNumber { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }
    public string BlockCorrelationId { get; private set; } = string.Empty;

    private ChainBlockAggregate() { }

    public static ChainBlockAggregate Create(
        Guid blockId,
        string previousHash,
        object payload,
        string payloadType,
        string payloadId,
        string correlationId,
        DateTimeOffset timestamp,
        long sequenceNumber)
    {
        Guard.AgainstDefault(blockId, nameof(blockId));
        Guard.AgainstEmpty(previousHash, nameof(previousHash));
        Guard.AgainstNull(payload, nameof(payload));
        Guard.AgainstEmpty(payloadType, nameof(payloadType));
        Guard.AgainstEmpty(payloadId, nameof(payloadId));
        Guard.AgainstEmpty(correlationId, nameof(correlationId));

        var serialized = HashService.SerializePayload(payload);
        if (string.IsNullOrWhiteSpace(serialized))
            throw ChainErrors.InvalidPayload();

        var computedPayloadHash = HashService.ComputeSHA256(serialized);

        // Hash = SHA256(PreviousHash + PayloadHash + SequenceNumber)
        // Aligned with ChainBlock.ComputeHash (infrastructure canonical formula).
        // Timestamp is metadata only — NOT part of hash computation.
        var hashInput =
            previousHash +
            computedPayloadHash +
            sequenceNumber;

        var computedCurrentHash = HashService.ComputeSHA256(hashInput);

        var block = new ChainBlockAggregate
        {
            Id = blockId,
            PreviousHash = HashValue.From(previousHash),
            PayloadHash = HashValue.From(computedPayloadHash),
            CurrentHash = HashValue.From(computedCurrentHash),
            Descriptor = PayloadDescriptor.From(payloadType, payloadId),
            SequenceNumber = sequenceNumber,
            Timestamp = timestamp,
            BlockCorrelationId = correlationId
        };

        block.RaiseDomainEvent(new ChainBlockCreatedEvent(
            BlockId: blockId.ToString(),
            PreviousHash: previousHash,
            CurrentHash: computedCurrentHash,
            PayloadHash: computedPayloadHash,
            PayloadType: payloadType,
            PayloadId: payloadId,
            Timestamp: timestamp,
            CorrelationIdValue: correlationId));

        return block;
    }
}
