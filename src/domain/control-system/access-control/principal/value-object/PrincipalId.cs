namespace Whycespace.Domain.ControlSystem.AccessControl.Principal;

public readonly record struct PrincipalId
{
    public string Value { get; }

    public PrincipalId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw PrincipalErrors.PrincipalIdMustNotBeEmpty();

        if (value.Length != 64 || !IsLowercaseHex(value))
            throw PrincipalErrors.PrincipalIdMustBe64HexChars(value);

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
