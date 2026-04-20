using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.File;

public readonly record struct DocumentFileMimeType
{
    public string Value { get; }

    public DocumentFileMimeType(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "DocumentFileMimeType cannot be empty.");
        Guard.Against(!value.Contains('/'), "DocumentFileMimeType must contain '/' (e.g. 'application/pdf').");
        Guard.Against(value.Length > 128, "DocumentFileMimeType cannot exceed 128 characters.");
        Value = value.Trim().ToLowerInvariant();
    }

    public override string ToString() => Value;
}
