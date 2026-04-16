using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Streaming.Playback;

public sealed class PlaybackSpecification : Specification<PlaybackStatus>
{
    private static readonly IReadOnlyDictionary<PlaybackStatus, IReadOnlySet<PlaybackStatus>> Allowed =
        new Dictionary<PlaybackStatus, IReadOnlySet<PlaybackStatus>>
        {
            [PlaybackStatus.Started] = new HashSet<PlaybackStatus> { PlaybackStatus.Paused, PlaybackStatus.Completed, PlaybackStatus.Stopped },
            [PlaybackStatus.Paused] = new HashSet<PlaybackStatus> { PlaybackStatus.Resumed, PlaybackStatus.Stopped },
            [PlaybackStatus.Resumed] = new HashSet<PlaybackStatus> { PlaybackStatus.Paused, PlaybackStatus.Completed, PlaybackStatus.Stopped },
            [PlaybackStatus.Completed] = new HashSet<PlaybackStatus>(),
            [PlaybackStatus.Stopped] = new HashSet<PlaybackStatus>()
        };

    public override bool IsSatisfiedBy(PlaybackStatus entity) =>
        entity == PlaybackStatus.Started || entity == PlaybackStatus.Resumed;

    public void EnsureTransition(PlaybackStatus from, PlaybackStatus to)
    {
        if (!Allowed.TryGetValue(from, out var set) || !set.Contains(to))
            throw PlaybackErrors.InvalidTransition(from, to);
    }
}
