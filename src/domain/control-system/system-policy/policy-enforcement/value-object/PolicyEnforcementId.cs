namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyEnforcement;

public readonly record struct PolicyEnforcementId
{
    public string Value { get; }

    public PolicyEnforcementId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw PolicyEnforcementErrors.EnforcementIdMustNotBeEmpty();

        if (value.Length != 64 || !IsLowercaseHex(value))
            throw PolicyEnforcementErrors.EnforcementIdMustBe64HexChars(value);

        Value = value;
    }

    private static bool IsLowercaseHex(string s)
    {
        foreach (var c in s)
            if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')))
                return false;
        return true;
    }

    public override string ToString() => Value;
}
