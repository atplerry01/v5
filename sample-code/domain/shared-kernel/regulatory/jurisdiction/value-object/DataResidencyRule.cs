namespace Whycespace.Domain.SharedKernel.Regulatory.Jurisdiction;

public sealed record DataResidencyRule
{
    public string RuleCode { get; }
    public string Description { get; }
    public bool RequiresLocalStorage { get; }

    public DataResidencyRule(string ruleCode, string description, bool requiresLocalStorage)
    {
        Guard.AgainstEmpty(ruleCode, nameof(ruleCode));
        Guard.AgainstEmpty(description, nameof(description));

        RuleCode = ruleCode;
        Description = description;
        RequiresLocalStorage = requiresLocalStorage;
    }
}
