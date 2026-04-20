using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Intake.Ingest;

public readonly record struct MediaIngestOutputRef
{
    public Guid Value { get; }

    public MediaIngestOutputRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "MediaIngestOutputRef cannot be empty.");
        Value = value;
    }
}
