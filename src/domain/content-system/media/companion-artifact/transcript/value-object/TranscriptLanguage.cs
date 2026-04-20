using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CompanionArtifact.Transcript;

/// BCP-47-style language tag for transcripts. Locally defined per
/// domain.guard.md rule 13 (no cross-BC type sharing).
public readonly record struct TranscriptLanguage
{
    public string Value { get; }

    public TranscriptLanguage(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "TranscriptLanguage cannot be empty.");
        Guard.Against(value.Length > 35, "TranscriptLanguage cannot exceed 35 characters (BCP-47).");
        var trimmed = value.Trim();
        Guard.Against(
            trimmed.Any(c => !(char.IsLetterOrDigit(c) || c == '-')),
            "TranscriptLanguage may contain only letters, digits, and dash.");
        Value = trimmed;
    }

    public override string ToString() => Value;
}
