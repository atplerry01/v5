namespace Whycespace.Domain.ControlSystem.Scheduling.ScheduleControl;

public readonly record struct ScheduleControlId
{
    public string Value { get; }

    public ScheduleControlId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw ScheduleControlErrors.IdMustNotBeEmpty();
        if (value.Length != 64 || !IsLowercaseHex(value))
            throw ScheduleControlErrors.IdMustBe64HexChars(value);
        Value = value;
    }

    private static bool IsLowercaseHex(string s)
    {
        foreach (var c in s) if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'))) return false;
        return true;
    }

    public override string ToString() => Value;
}
