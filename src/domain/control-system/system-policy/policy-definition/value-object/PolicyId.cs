namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyDefinition;

public readonly record struct PolicyId
{
    public string Value { get; }

    public PolicyId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw PolicyDefinitionErrors.PolicyIdMustNotBeEmpty();

        if (value.Length != 64 || !IsLowercaseHex(value))
            throw PolicyDefinitionErrors.PolicyIdMustBe64HexChars(value);

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
