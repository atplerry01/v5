using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Engagement.Feed;

public sealed record FeedItem : ValueObject
{
    public string ItemRef { get; }
    public int Rank { get; }
    public Timestamp AppendedAt { get; }

    private FeedItem(string itemRef, int rank, Timestamp at)
    {
        ItemRef = itemRef;
        Rank = rank;
        AppendedAt = at;
    }

    public static FeedItem Create(string itemRef, int rank, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(itemRef)) throw FeedErrors.InvalidItem();
        if (rank < 0) throw FeedErrors.InvalidRank();
        return new FeedItem(itemRef, rank, at);
    }
}
