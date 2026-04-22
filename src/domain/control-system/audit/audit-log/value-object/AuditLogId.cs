namespace Whycespace.Domain.ControlSystem.Audit.AuditLog;

public readonly record struct AuditLogId
{
    public string Value { get; }

    public AuditLogId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw AuditLogErrors.IdMustNotBeEmpty();
        if (value.Length != 64 || !IsLowercaseHex(value))
            throw AuditLogErrors.IdMustBe64HexChars(value);
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
