namespace Whycespace.Domain.ContentSystem.Engagement.Community;

public readonly record struct CommunityId(Guid Value)
{
    public static CommunityId From(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}
