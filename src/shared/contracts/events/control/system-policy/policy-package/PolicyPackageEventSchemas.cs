namespace Whycespace.Shared.Contracts.Events.Control.SystemPolicy.PolicyPackage;

public sealed record PolicyPackageAssembledEventSchema(
    Guid AggregateId,
    string Name,
    int VersionMajor,
    int VersionMinor,
    IReadOnlyList<string> PolicyDefinitionIds);

public sealed record PolicyPackageDeployedEventSchema(
    Guid AggregateId,
    int VersionMajor,
    int VersionMinor);

public sealed record PolicyPackageRetiredEventSchema(
    Guid AggregateId);
