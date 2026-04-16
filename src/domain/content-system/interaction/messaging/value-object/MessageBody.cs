using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Interaction.Messaging;

public sealed record MessageBody : ValueObject
{
    public const int MaxLength = 8_000;
    public string Value { get; }
    private MessageBody(string value) => Value = value;

    public static MessageBody Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw MessagingErrors.InvalidBody();
        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw MessagingErrors.BodyTooLong(MaxLength);
        return new MessageBody(trimmed);
    }

    public override string ToString() => Value;
}
