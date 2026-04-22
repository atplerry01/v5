namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyDecision;

public readonly record struct PolicyDecisionId
{
    public string Value { get; }

    public PolicyDecisionId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw PolicyDecisionErrors.DecisionIdMustNotBeEmpty();

        if (value.Length != 64 || !IsLowercaseHex(value))
            throw PolicyDecisionErrors.DecisionIdMustBe64HexChars(value);

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
