namespace Whycespace.Domain.ContentSystem.Engagement.Feed;

public static class FeedOrderingService
{
    public static IReadOnlyList<FeedItem> OrderDescending(IEnumerable<FeedItem> items)
    {
        if (items is null) return Array.Empty<FeedItem>();
        return items
            .OrderByDescending(i => i.Rank)
            .ThenByDescending(i => i.AppendedAt.Value)
            .ThenBy(i => i.ItemRef, StringComparer.Ordinal)
            .ToList();
    }

    public static IReadOnlyList<FeedItem> TakeTop(IEnumerable<FeedItem> items, int count)
    {
        if (count <= 0) return Array.Empty<FeedItem>();
        return OrderDescending(items).Take(count).ToList();
    }
}
