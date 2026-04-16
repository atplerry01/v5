namespace Whycespace.Domain.ContentSystem.Engagement.Reaction;

public readonly record struct ReactionId(Guid Value)
{
    public static ReactionId From(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}
