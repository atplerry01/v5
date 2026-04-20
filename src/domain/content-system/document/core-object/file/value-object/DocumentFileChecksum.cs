using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.File;

public readonly record struct DocumentFileChecksum
{
    public string Value { get; }

    public DocumentFileChecksum(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "DocumentFileChecksum cannot be empty.");
        Guard.Against(value.Length != 64, "DocumentFileChecksum must be a 64-char SHA256 hex string.");
        Value = value.ToLowerInvariant();
    }

    public override string ToString() => Value;
}
