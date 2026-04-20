using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Descriptor.Metadata;

/// BCP-47-style language tag (e.g. "en", "en-US", "fr-CA").
public readonly record struct LanguageTag
{
    public string Value { get; }

    public LanguageTag(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "LanguageTag cannot be empty.");
        Guard.Against(value.Length > 35, "LanguageTag cannot exceed 35 characters (BCP-47).");
        var trimmed = value.Trim();
        Guard.Against(
            trimmed.Any(c => !(char.IsLetterOrDigit(c) || c == '-')),
            "LanguageTag may contain only letters, digits, and dash.");
        Value = trimmed;
    }

    public override string ToString() => Value;
}
