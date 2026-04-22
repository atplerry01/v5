namespace Whycespace.Domain.ControlSystem.AccessControl.Role;

public readonly record struct RoleId
{
    public string Value { get; }

    public RoleId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw RoleErrors.IdMustNotBeEmpty();
        if (value.Length != 64 || !IsLowercaseHex(value))
            throw RoleErrors.IdMustBe64HexChars(value);
        Value = value;
    }

    private static bool IsLowercaseHex(string s)
    {
        foreach (var c in s) if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'))) return false;
        return true;
    }

    public override string ToString() => Value;
}
