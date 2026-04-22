namespace Whycespace.Domain.ControlSystem.AccessControl.Permission;

public readonly record struct PermissionId
{
    public string Value { get; }

    public PermissionId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw PermissionErrors.IdMustNotBeEmpty();
        if (value.Length != 64 || !IsLowercaseHex(value))
            throw PermissionErrors.IdMustBe64HexChars(value);
        Value = value;
    }

    private static bool IsLowercaseHex(string s)
    {
        foreach (var c in s) if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'))) return false;
        return true;
    }

    public override string ToString() => Value;
}
