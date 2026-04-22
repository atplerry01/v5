namespace Whycespace.Domain.PlatformSystem.Schema.Contract;

public readonly record struct ContractStatus
{
    public static readonly ContractStatus Active = new("Active");
    public static readonly ContractStatus Deprecated = new("Deprecated");

    public string Value { get; }

    private ContractStatus(string value) => Value = value;
}
