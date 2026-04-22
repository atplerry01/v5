namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyAudit;

public readonly record struct PolicyAuditId
{
    public string Value { get; }

    public PolicyAuditId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw PolicyAuditErrors.AuditIdMustNotBeEmpty();

        if (value.Length != 64 || !IsLowercaseHex(value))
            throw PolicyAuditErrors.AuditIdMustBe64HexChars(value);

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
