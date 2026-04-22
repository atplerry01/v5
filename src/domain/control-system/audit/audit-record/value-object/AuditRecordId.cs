namespace Whycespace.Domain.ControlSystem.Audit.AuditRecord;

public readonly record struct AuditRecordId
{
    public string Value { get; }

    public AuditRecordId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw AuditRecordErrors.IdMustNotBeEmpty();
        if (value.Length != 64 || !IsLowercaseHex(value))
            throw AuditRecordErrors.IdMustBe64HexChars(value);
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
