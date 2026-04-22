namespace Whycespace.Domain.ControlSystem.AccessControl.Identity;

public readonly record struct IdentityId
{
    public string Value { get; }

    public IdentityId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw IdentityErrors.IdentityIdMustNotBeEmpty();

        if (value.Length != 64 || !IsLowercaseHex(value))
            throw IdentityErrors.IdentityIdMustBe64HexChars(value);

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
