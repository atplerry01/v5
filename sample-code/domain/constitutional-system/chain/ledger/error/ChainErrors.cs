namespace Whycespace.Domain.ConstitutionalSystem.Chain.Ledger;

public static class ChainErrors
{
    public static DomainException InvalidHash()
        => new("CHAIN.INVALID_HASH", "The computed hash does not match the expected value.");

    public static DomainException BrokenChain(string blockId, string expectedPreviousHash, string actualPreviousHash)
        => new("CHAIN.BROKEN_CHAIN",
            $"Block '{blockId}' previous hash mismatch. Expected '{expectedPreviousHash}', got '{actualPreviousHash}'.");

    public static DomainException DuplicateBlock(string blockId)
        => new("CHAIN.DUPLICATE_BLOCK", $"A block with id '{blockId}' already exists in the chain.");

    public static DomainException InvalidPayload()
        => new("CHAIN.INVALID_PAYLOAD", "The payload is null, empty, or cannot be serialized deterministically.");

    public static DomainException ChainHeadMismatch(string expectedHash, string actualHash)
        => new("CHAIN.HEAD_MISMATCH",
            $"Chain head mismatch — caller expected '{expectedHash}' but actual head is '{actualHash}'. Possible concurrent fork attempt.");
}
