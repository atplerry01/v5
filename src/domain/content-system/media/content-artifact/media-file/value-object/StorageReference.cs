using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.MediaFile;

public readonly record struct StorageReference
{
    public string Value { get; }

    public StorageReference(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "StorageReference cannot be empty.");
        Guard.Against(value.Length > 1024, "StorageReference cannot exceed 1024 characters.");
        Value = value.Trim();
    }

    public override string ToString() => Value;
}
