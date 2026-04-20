using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.Template;

public readonly record struct TemplateName
{
    public string Value { get; }

    public TemplateName(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "TemplateName cannot be empty.");
        Guard.Against(value.Length > 256, "TemplateName cannot exceed 256 characters.");
        Value = value.Trim();
    }

    public override string ToString() => Value;
}
