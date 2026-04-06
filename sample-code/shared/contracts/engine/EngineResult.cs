namespace Whycespace.Shared.Contracts.Engine;

public sealed record EngineResult
{
    public required bool Success { get; init; }
    public object? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ErrorCode { get; init; }

    public static EngineResult Ok(object? data = null) =>
        new() { Success = true, Data = data };

    public static EngineResult Fail(string message, string? code = null) =>
        new() { Success = false, ErrorMessage = message, ErrorCode = code };
}
