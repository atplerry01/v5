namespace Whycespace.Domain.ContentSystem.Media.Streaming.StreamSession;

public readonly record struct StreamSessionId(Guid Value)
{
    public static StreamSessionId From(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}
