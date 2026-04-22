namespace Whycespace.Domain.ControlSystem.AccessControl.Authorization;

public readonly record struct AuthorizationId
{
    public string Value { get; }

    public AuthorizationId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw AuthorizationErrors.IdMustNotBeEmpty();
        if (value.Length != 64 || !IsLowercaseHex(value))
            throw AuthorizationErrors.IdMustBe64HexChars(value);
        Value = value;
    }

    private static bool IsLowercaseHex(string s)
    {
        foreach (var c in s) if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'))) return false;
        return true;
    }

    public override string ToString() => Value;
}
