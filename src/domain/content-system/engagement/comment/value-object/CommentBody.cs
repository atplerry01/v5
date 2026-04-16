using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Engagement.Comment;

public sealed record CommentBody : ValueObject
{
    public const int MaxLength = 10_000;
    public string Value { get; }
    private CommentBody(string v) => Value = v;
    public static CommentBody Create(string v)
    {
        if (string.IsNullOrWhiteSpace(v)) throw CommentErrors.InvalidBody();
        var t = v.Trim();
        if (t.Length > MaxLength) throw CommentErrors.BodyTooLong(MaxLength);
        return new CommentBody(t);
    }
    public override string ToString() => Value;
}
