namespace Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyPackage;

public sealed record PolicyPackageReadModel
{
    public Guid PackageId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int VersionMajor { get; init; }
    public int VersionMinor { get; init; }
    public IReadOnlyList<string> PolicyDefinitionIds { get; init; } = [];
    public string Status { get; init; } = string.Empty;
}
