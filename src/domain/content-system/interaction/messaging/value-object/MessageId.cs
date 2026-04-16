namespace Whycespace.Domain.ContentSystem.Interaction.Messaging;

public readonly record struct MessageId(Guid Value)
{
    public static MessageId From(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}
