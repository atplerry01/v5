namespace Whycespace.Domain.ContentSystem.Engagement.Feed;

public readonly record struct FeedId(Guid Value)
{
    public static FeedId From(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}
