namespace Whycespace.Domain.ControlSystem.SystemReconciliation.ReconciliationRun;

public readonly record struct ReconciliationRunId
{
    public string Value { get; }

    public ReconciliationRunId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw ReconciliationRunErrors.RunIdMustNotBeEmpty();

        if (value.Length != 64 || !IsLowercaseHex(value))
            throw ReconciliationRunErrors.RunIdMustBe64HexChars(value);

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
