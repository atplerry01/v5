namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyEvaluation;

public readonly record struct PolicyEvaluationId
{
    public string Value { get; }

    public PolicyEvaluationId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw PolicyEvaluationErrors.EvaluationIdMustNotBeEmpty();

        if (value.Length != 64 || !IsLowercaseHex(value))
            throw PolicyEvaluationErrors.EvaluationIdMustBe64HexChars(value);

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
