using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.Document;

public readonly record struct DocumentTitle
{
    public string Value { get; }

    public DocumentTitle(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "DocumentTitle cannot be empty.");
        Guard.Against(value.Length > 256, "DocumentTitle cannot exceed 256 characters.");
        Value = value.Trim();
    }

    public override string ToString() => Value;
}
