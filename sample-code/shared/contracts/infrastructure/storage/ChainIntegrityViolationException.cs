namespace Whycespace.Shared.Contracts.Infrastructure.Storage;

/// <summary>
/// Thrown when WhyceChain tamper detection identifies a hash mismatch.
/// computed hash != stored hash → CHAIN_INTEGRITY_VIOLATION.
/// </summary>
public sealed class ChainIntegrityViolationException : Exception
{
    public string BlockId { get; }
    public long SequenceNumber { get; }
    public string StoredHash { get; }
    public string ComputedHash { get; }

    public ChainIntegrityViolationException(
        string blockId,
        long sequenceNumber,
        string storedHash,
        string computedHash)
        : base($"CHAIN_INTEGRITY_VIOLATION: Block {blockId} (seq {sequenceNumber}) — stored hash [{storedHash}] does not match computed hash [{computedHash}].")
    {
        BlockId = blockId;
        SequenceNumber = sequenceNumber;
        StoredHash = storedHash;
        ComputedHash = computedHash;
    }
}
