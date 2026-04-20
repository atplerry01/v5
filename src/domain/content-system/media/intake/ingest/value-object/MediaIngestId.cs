using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Intake.Ingest;

public readonly record struct MediaIngestId
{
    public Guid Value { get; }

    public MediaIngestId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "MediaIngestId cannot be empty.");
        Value = value;
    }
}
