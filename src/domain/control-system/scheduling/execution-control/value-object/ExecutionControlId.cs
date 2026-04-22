namespace Whycespace.Domain.ControlSystem.Scheduling.ExecutionControl;

public readonly record struct ExecutionControlId
{
    public string Value { get; }

    public ExecutionControlId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw ExecutionControlErrors.IdMustNotBeEmpty();
        if (value.Length != 64 || !IsLowercaseHex(value))
            throw ExecutionControlErrors.IdMustBe64HexChars(value);
        Value = value;
    }

    private static bool IsLowercaseHex(string s)
    {
        foreach (var c in s) if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'))) return false;
        return true;
    }

    public override string ToString() => Value;
}
