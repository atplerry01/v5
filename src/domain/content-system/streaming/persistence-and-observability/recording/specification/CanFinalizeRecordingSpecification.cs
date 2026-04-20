using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PersistenceAndObservability.Recording;

public sealed class CanFinalizeRecordingSpecification : Specification<RecordingAggregate>
{
    public override bool IsSatisfiedBy(RecordingAggregate entity)
        => entity.Status == RecordingStatus.Completed;
}
