using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Streaming.StreamSession;

public sealed class StreamSessionSpecification : Specification<StreamSessionStatus>
{
    public override bool IsSatisfiedBy(StreamSessionStatus entity) => entity == StreamSessionStatus.Open;

    public void EnsureOpen(StreamSessionStatus status)
    {
        if (status != StreamSessionStatus.Open) throw StreamSessionErrors.SessionNotOpen();
    }

    public void EnsureNotTerminal(StreamSessionStatus status)
    {
        if (status == StreamSessionStatus.Closed || status == StreamSessionStatus.Terminated)
            throw StreamSessionErrors.AlreadyTerminal();
    }
}
