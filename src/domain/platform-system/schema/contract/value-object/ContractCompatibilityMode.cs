namespace Whycespace.Domain.PlatformSystem.Schema.Contract;

public readonly record struct ContractCompatibilityMode
{
    public static readonly ContractCompatibilityMode Backward = new("Backward");
    public static readonly ContractCompatibilityMode Forward = new("Forward");
    public static readonly ContractCompatibilityMode Full = new("Full");
    public static readonly ContractCompatibilityMode None = new("None");

    public string Value { get; }

    private ContractCompatibilityMode(string value) => Value = value;
}
