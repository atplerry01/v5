namespace Whycespace.Domain.ControlSystem.SystemReconciliation.DiscrepancyDetection;

public readonly record struct DiscrepancyDetectionId
{
    public string Value { get; }

    public DiscrepancyDetectionId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw DiscrepancyDetectionErrors.DetectionIdMustNotBeEmpty();

        if (value.Length != 64 || !IsLowercaseHex(value))
            throw DiscrepancyDetectionErrors.DetectionIdMustBe64HexChars(value);

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
