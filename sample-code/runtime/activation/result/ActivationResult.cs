namespace Whycespace.Runtime.Activation.Result;

public sealed record ActivationResult
{
    public required string Status { get; init; }
    public required string Mode { get; init; }
    public string? Reason { get; init; }

    public static ActivationResult Simulation() =>
        new() { Status = "SIMULATION", Mode = "SIMULATION" };

    public static ActivationResult Blocked(string reason) =>
        new() { Status = "BLOCKED", Mode = "NONE", Reason = reason };

    public static ActivationResult Active(string mode) =>
        new() { Status = "ACTIVE", Mode = mode };
}
