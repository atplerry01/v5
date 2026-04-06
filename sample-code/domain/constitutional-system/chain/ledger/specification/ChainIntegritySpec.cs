namespace Whycespace.Domain.ConstitutionalSystem.Chain.Ledger;

public sealed class ChainIntegritySpec : Specification<ChainBlockAggregate>
{
    private readonly string _expectedPreviousHash;
    private readonly HashSet<string> _existingBlockIds;

    public ChainIntegritySpec(string expectedPreviousHash, IEnumerable<string> existingBlockIds)
    {
        _expectedPreviousHash = expectedPreviousHash;
        _existingBlockIds = new HashSet<string>(existingBlockIds);
    }

    public override bool IsSatisfiedBy(ChainBlockAggregate block)
    {
        if (_existingBlockIds.Contains(block.Id.ToString()))
            throw ChainErrors.DuplicateBlock(block.Id.ToString());

        if (block.PreviousHash.Value != _expectedPreviousHash)
            throw ChainErrors.BrokenChain(block.Id.ToString(), _expectedPreviousHash, block.PreviousHash.Value);

        // Hash = SHA256(PreviousHash + PayloadHash + SequenceNumber)
        // Aligned with ChainBlock.ComputeHash (infrastructure canonical formula).
        // Timestamp is metadata only — NOT part of hash computation.
        var recomputedHash = HashService.ComputeSHA256(
            block.PreviousHash.Value +
            block.PayloadHash.Value +
            block.SequenceNumber);

        if (block.CurrentHash.Value != recomputedHash)
            throw ChainErrors.InvalidHash();

        return true;
    }
}
