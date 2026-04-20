using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.File;

public readonly record struct DocumentFileId
{
    public Guid Value { get; }

    public DocumentFileId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "DocumentFileId cannot be empty.");
        Value = value;
    }
}
