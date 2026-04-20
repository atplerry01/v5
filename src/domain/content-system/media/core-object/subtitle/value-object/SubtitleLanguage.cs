using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Subtitle;

/// BCP-47-style language tag for subtitles. Locally defined per
/// domain.guard.md rule 13 (no cross-BC type sharing).
public readonly record struct SubtitleLanguage
{
    public string Value { get; }

    public SubtitleLanguage(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "SubtitleLanguage cannot be empty.");
        Guard.Against(value.Length > 35, "SubtitleLanguage cannot exceed 35 characters (BCP-47).");
        var trimmed = value.Trim();
        Guard.Against(
            trimmed.Any(c => !(char.IsLetterOrDigit(c) || c == '-')),
            "SubtitleLanguage may contain only letters, digits, and dash.");
        Value = trimmed;
    }

    public override string ToString() => Value;
}
