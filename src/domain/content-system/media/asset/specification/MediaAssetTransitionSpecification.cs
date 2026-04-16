using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Asset;

public sealed record MediaAssetTransitionRequest(
    MediaAssetStatus From,
    MediaAssetStatus To);

public sealed class MediaAssetTransitionSpecification : Specification<MediaAssetTransitionRequest>
{
    private static readonly IReadOnlyDictionary<MediaAssetStatus, IReadOnlySet<MediaAssetStatus>> Allowed =
        new Dictionary<MediaAssetStatus, IReadOnlySet<MediaAssetStatus>>
        {
            [MediaAssetStatus.Draft] = new HashSet<MediaAssetStatus>
            {
                MediaAssetStatus.Processing,
                MediaAssetStatus.Archived
            },
            [MediaAssetStatus.Processing] = new HashSet<MediaAssetStatus>
            {
                MediaAssetStatus.Available,
                MediaAssetStatus.Archived
            },
            [MediaAssetStatus.Available] = new HashSet<MediaAssetStatus>
            {
                MediaAssetStatus.Published,
                MediaAssetStatus.Archived
            },
            [MediaAssetStatus.Published] = new HashSet<MediaAssetStatus>
            {
                MediaAssetStatus.Available,
                MediaAssetStatus.Archived
            },
            [MediaAssetStatus.Archived] = new HashSet<MediaAssetStatus>()
        };

    public override bool IsSatisfiedBy(MediaAssetTransitionRequest entity) =>
        entity is not null
        && Allowed.TryGetValue(entity.From, out var destinations)
        && destinations.Contains(entity.To);

    public void EnsureSatisfied(MediaAssetStatus from, MediaAssetStatus to)
    {
        if (!IsSatisfiedBy(new MediaAssetTransitionRequest(from, to)))
            throw MediaAssetErrors.InvalidTransition(from, to);
    }
}
