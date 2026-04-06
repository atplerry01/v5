namespace Whycespace.Engines.T2E.Trust.Identity.IdentityGraph;

public record IdentityGraphResult(bool Success, string Message);
public sealed record IdentityGraphDto(string GraphId, string PrimaryIdentityId, string Status, int LinkCount);
