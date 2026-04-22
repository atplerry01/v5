namespace Whycespace.Domain.ControlSystem.SystemReconciliation.SystemVerification;

public readonly record struct SystemVerificationId
{
    public string Value { get; }

    public SystemVerificationId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw SystemVerificationErrors.VerificationIdMustNotBeEmpty();

        if (value.Length != 64 || !IsLowercaseHex(value))
            throw SystemVerificationErrors.VerificationIdMustBe64HexChars(value);

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
