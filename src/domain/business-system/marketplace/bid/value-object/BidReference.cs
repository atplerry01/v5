namespace Whycespace.Domain.BusinessSystem.Marketplace.Bid;

public readonly record struct BidReference
{
    public Guid TargetId { get; }
    public string TargetType { get; }

    public BidReference(Guid targetId, string targetType)
    {
        if (targetId == Guid.Empty)
            throw new ArgumentException("BidReference target must not be empty.", nameof(targetId));

        if (string.IsNullOrWhiteSpace(targetType))
            throw new ArgumentException("BidReference target type must not be empty.", nameof(targetType));

        TargetId = targetId;
        TargetType = targetType;
    }
}
