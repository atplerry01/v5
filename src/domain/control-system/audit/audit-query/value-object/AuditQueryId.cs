namespace Whycespace.Domain.ControlSystem.Audit.AuditQuery;

public readonly record struct AuditQueryId
{
    public string Value { get; }

    public AuditQueryId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw AuditQueryErrors.QueryIdMustNotBeEmpty();

        if (value.Length != 64 || !IsLowercaseHex(value))
            throw AuditQueryErrors.QueryIdMustBe64HexChars(value);

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
