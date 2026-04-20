using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.Bundle;

public readonly record struct DocumentBundleId
{
    public Guid Value { get; }

    public DocumentBundleId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "DocumentBundleId cannot be empty.");
        Value = value;
    }
}
