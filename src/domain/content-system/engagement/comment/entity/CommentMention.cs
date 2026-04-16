namespace Whycespace.Domain.ContentSystem.Engagement.Comment;

public sealed class CommentMention
{
    public string MentionedRef { get; }
    public int Offset { get; }
    public int Length { get; }

    private CommentMention(string mentionedRef, int offset, int length)
    {
        MentionedRef = mentionedRef;
        Offset = offset;
        Length = length;
    }

    public static CommentMention Create(string mentionedRef, int offset, int length)
    {
        if (string.IsNullOrWhiteSpace(mentionedRef)) throw CommentErrors.InvalidMention();
        if (offset < 0 || length <= 0) throw CommentErrors.InvalidMention();
        return new CommentMention(mentionedRef, offset, length);
    }
}
