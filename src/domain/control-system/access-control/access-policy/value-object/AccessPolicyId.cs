namespace Whycespace.Domain.ControlSystem.AccessControl.AccessPolicy;

public readonly record struct AccessPolicyId
{
    public string Value { get; }

    public AccessPolicyId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw AccessPolicyErrors.AccessPolicyIdMustNotBeEmpty();

        if (value.Length != 64 || !IsLowercaseHex(value))
            throw AccessPolicyErrors.AccessPolicyIdMustBe64HexChars(value);

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
