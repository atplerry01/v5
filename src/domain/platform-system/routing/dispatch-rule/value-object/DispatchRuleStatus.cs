namespace Whycespace.Domain.PlatformSystem.Routing.DispatchRule;

public readonly record struct DispatchRuleStatus
{
    public static readonly DispatchRuleStatus Active = new("Active");
    public static readonly DispatchRuleStatus Inactive = new("Inactive");

    public string Value { get; }

    private DispatchRuleStatus(string value) => Value = value;
}
