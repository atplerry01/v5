namespace Whycespace.Domain.ContentSystem.Interaction.Presence;

public readonly record struct PresenceId(Guid Value)
{
    public static PresenceId From(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}
