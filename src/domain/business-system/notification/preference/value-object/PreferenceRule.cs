namespace Whycespace.Domain.BusinessSystem.Notification.Preference;

public readonly record struct PreferenceRule
{
    public Guid OwnerReference { get; }
    public string RuleName { get; }

    public PreferenceRule(Guid ownerReference, string ruleName)
    {
        if (ownerReference == Guid.Empty)
            throw new ArgumentException("Owner reference must not be empty.", nameof(ownerReference));

        if (string.IsNullOrWhiteSpace(ruleName))
            throw new ArgumentException("Rule name must not be empty.", nameof(ruleName));

        OwnerReference = ownerReference;
        RuleName = ruleName;
    }
}
