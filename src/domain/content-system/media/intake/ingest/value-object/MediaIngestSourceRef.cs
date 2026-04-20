using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Intake.Ingest;

/// Reference to the media asset the upload is bound to. Bare-id to avoid
/// cross-BC type imports per domain.guard.md rule 13.
public readonly record struct MediaIngestSourceRef
{
    public Guid Value { get; }

    public MediaIngestSourceRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "MediaIngestSourceRef cannot be empty.");
        Value = value;
    }
}
