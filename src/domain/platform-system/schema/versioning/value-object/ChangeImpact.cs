namespace Whycespace.Domain.PlatformSystem.Schema.Versioning;

public readonly record struct ChangeImpact
{
    public static readonly ChangeImpact Safe = new("Safe");
    public static readonly ChangeImpact RequiresConsumerUpdate = new("RequiresConsumerUpdate");
    public static readonly ChangeImpact Breaking = new("Breaking");

    public string Value { get; }

    private ChangeImpact(string value) => Value = value;
}
