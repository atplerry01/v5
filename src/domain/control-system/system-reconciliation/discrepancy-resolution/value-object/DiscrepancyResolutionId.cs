namespace Whycespace.Domain.ControlSystem.SystemReconciliation.DiscrepancyResolution;

public readonly record struct DiscrepancyResolutionId
{
    public string Value { get; }

    public DiscrepancyResolutionId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw DiscrepancyResolutionErrors.ResolutionIdMustNotBeEmpty();

        if (value.Length != 64 || !IsLowercaseHex(value))
            throw DiscrepancyResolutionErrors.ResolutionIdMustBe64HexChars(value);

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
