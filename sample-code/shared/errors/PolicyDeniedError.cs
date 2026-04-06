namespace Whycespace.Shared.Errors;

public sealed record PolicyDeniedError
{
    public const string ErrorCode = "POLICY_DENIED";

    public required IReadOnlyList<Guid> PolicyIds { get; init; }
    public required IReadOnlyList<string> ViolatedRules { get; init; }
    public required string Reason { get; init; }
    public required string CorrelationId { get; init; }

    public string ToErrorMessage()
    {
        if (ViolatedRules.Count == 0)
            return Reason;

        return ViolatedRules.Count == 1
            ? $"{Reason} — Rule: {ViolatedRules[0]}"
            : $"{Reason} — Rules: {string.Join("; ", ViolatedRules)}";
    }
}
