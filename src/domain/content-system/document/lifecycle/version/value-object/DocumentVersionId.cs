using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Version;

public readonly record struct DocumentVersionId
{
    public Guid Value { get; }

    public DocumentVersionId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "DocumentVersionId cannot be empty.");
        Value = value;
    }
}
