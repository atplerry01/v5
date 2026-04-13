namespace Whycespace.Shared.Contracts.Common;

/// <summary>
/// Acknowledgment response for command endpoints that mutate state
/// but return no domain-specific data beyond the operation status.
/// </summary>
public sealed record CommandAck(string Status);
