namespace Whycespace.Domain.ConstitutionalSystem.Chain.AnchorRecord;

public static class AnchorRecordErrors
{
    public static InvalidOperationException AlreadySealed() =>
        new("Anchor record is already sealed.");

    public static ArgumentException InvalidBlockHash() =>
        new("Block hash must be non-empty.", nameof(AnchorDescriptor));

    public static ArgumentException InvalidSequence() =>
        new("Chain sequence must be non-negative.", nameof(AnchorDescriptor));
}
