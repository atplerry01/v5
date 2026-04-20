using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Audio;

public sealed class CanActivateAudioSpecification : Specification<AudioAggregate>
{
    public override bool IsSatisfiedBy(AudioAggregate entity)
        => entity.Status == AudioStatus.Draft;
}
