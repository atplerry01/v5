namespace Whycespace.Domain.ContentSystem.Engagement.Search;

public readonly record struct SearchIndexId(Guid Value)
{
    public static SearchIndexId From(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}
