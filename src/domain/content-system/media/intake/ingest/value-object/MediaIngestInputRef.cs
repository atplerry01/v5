using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Intake.Ingest;

public readonly record struct MediaIngestInputRef
{
    public Guid Value { get; }

    public MediaIngestInputRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "MediaIngestInputRef cannot be empty.");
        Value = value;
    }
}
