namespace Whycespace.Domain.ContentSystem.Interaction.Conversation;

public readonly record struct ConversationId(Guid Value)
{
    public static ConversationId From(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}
