namespace Whycespace.Domain.ContentSystem.Interaction.Call;

public readonly record struct CallId(Guid Value)
{
    public static CallId From(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}
