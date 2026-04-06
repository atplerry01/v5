namespace Whycespace.Shared.Primitives.Id;

public sealed class IdGenerationContext
{
    public required string Domain { get; init; }
    public required string Type { get; init; }
    public required string Jurisdiction { get; init; }
    public required DateTime Timestamp { get; init; }
    public string? DeterministicKey { get; init; }
}
