using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Channel;

public readonly record struct ChannelName
{
    public string Value { get; }

    public ChannelName(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "ChannelName cannot be empty.");
        Guard.Against(value.Length > 128, "ChannelName cannot exceed 128 characters.");
        var trimmed = value.Trim();
        Guard.Against(
            trimmed.Any(c => !(char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '.')),
            "ChannelName may contain only letters, digits, dot, dash, or underscore.");
        Value = trimmed.ToLowerInvariant();
    }

    public override string ToString() => Value;
}
