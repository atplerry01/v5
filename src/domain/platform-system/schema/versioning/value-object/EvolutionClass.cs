namespace Whycespace.Domain.PlatformSystem.Schema.Versioning;

public readonly record struct EvolutionClass
{
    public static readonly EvolutionClass NonBreaking = new("NonBreaking");
    public static readonly EvolutionClass Breaking = new("Breaking");
    public static readonly EvolutionClass Incompatible = new("Incompatible");

    public string Value { get; }

    private EvolutionClass(string value) => Value = value;
}
