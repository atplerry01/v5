using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.File;

public readonly record struct DocumentFileStorageRef
{
    public string Value { get; }

    public DocumentFileStorageRef(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "DocumentFileStorageRef cannot be empty.");
        Guard.Against(value.Length > 1024, "DocumentFileStorageRef cannot exceed 1024 characters.");
        Value = value.Trim();
    }

    public override string ToString() => Value;
}
