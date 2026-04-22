namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyPackage;

public readonly record struct PolicyPackageId
{
    public string Value { get; }

    public PolicyPackageId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw PolicyPackageErrors.PackageIdMustNotBeEmpty();

        if (value.Length != 64 || !IsLowercaseHex(value))
            throw PolicyPackageErrors.PackageIdMustBe64HexChars(value);

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
