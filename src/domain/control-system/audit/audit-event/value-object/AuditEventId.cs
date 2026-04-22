namespace Whycespace.Domain.ControlSystem.Audit.AuditEvent;

public readonly record struct AuditEventId
{
    public string Value { get; }

    public AuditEventId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw AuditEventErrors.AuditEventIdMustNotBeEmpty();

        if (value.Length != 64 || !IsLowercaseHex(value))
            throw AuditEventErrors.AuditEventIdMustBe64HexChars(value);

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
