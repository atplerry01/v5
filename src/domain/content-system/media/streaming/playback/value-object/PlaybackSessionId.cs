namespace Whycespace.Domain.ContentSystem.Media.Streaming.Playback;

public readonly record struct PlaybackSessionId(Guid Value)
{
    public static PlaybackSessionId From(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}
