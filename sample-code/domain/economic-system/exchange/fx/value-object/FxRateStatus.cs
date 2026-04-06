using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Exchange.Fx;

public sealed class FxRateStatus : ValueObject
{
    public static readonly FxRateStatus Pending = new("Pending");
    public static readonly FxRateStatus Active = new("Active");
    public static readonly FxRateStatus Superseded = new("Superseded");
    public static readonly FxRateStatus Invalid = new("Invalid");

    public string Value { get; }
    private FxRateStatus(string value) => Value = value;
    public bool IsTerminal => this == Superseded || this == Invalid;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
