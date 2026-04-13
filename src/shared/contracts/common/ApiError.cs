namespace Whyce.Shared.Contracts.Common;

/// <summary>
/// Standard error payload within an API response envelope.
/// </summary>
public sealed record ApiError(string Code, string Message);
