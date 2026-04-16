namespace Whycespace.Domain.ContentSystem.Engagement.Comment;

public readonly record struct CommentId(Guid Value)
{
    public static CommentId From(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}
