namespace Whycespace.Domain.ControlSystem.Configuration.ConfigurationAssignment;

public readonly record struct ConfigurationAssignmentId
{
    public string Value { get; }

    public ConfigurationAssignmentId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw ConfigurationAssignmentErrors.AssignmentIdMustNotBeEmpty();

        if (value.Length != 64 || !IsLowercaseHex(value))
            throw ConfigurationAssignmentErrors.AssignmentIdMustBe64HexChars(value);

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
