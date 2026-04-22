namespace Whycespace.Domain.ControlSystem.Audit.AuditTrace;

public readonly record struct AuditTraceId
{
    public string Value { get; }

    public AuditTraceId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw AuditTraceErrors.TraceIdMustNotBeEmpty();

        if (value.Length != 64 || !IsLowercaseHex(value))
            throw AuditTraceErrors.TraceIdMustBe64HexChars(value);

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
