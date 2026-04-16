using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Interaction.Conversation;

public sealed record ConversationTopic : ValueObject
{
    public const int MaxLength = 200;
    public string Value { get; }
    private ConversationTopic(string value) => Value = value;

    public static ConversationTopic Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw ConversationErrors.InvalidTopic();
        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw ConversationErrors.TopicTooLong(MaxLength);
        return new ConversationTopic(trimmed);
    }

    public override string ToString() => Value;
}
