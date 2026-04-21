namespace Whycespace.Shared.Contracts.Events.Structural.Cluster.Cluster;

public sealed record ClusterDefinedEventSchema(
    Guid AggregateId,
    string ClusterName,
    string ClusterType);

public sealed record ClusterActivatedEventSchema(
    Guid AggregateId);

public sealed record ClusterArchivedEventSchema(
    Guid AggregateId);

public sealed record ClusterAuthorityBoundEventSchema(
    Guid AggregateId,
    Guid AuthorityId);

public sealed record ClusterAuthorityReleasedEventSchema(
    Guid AggregateId,
    Guid AuthorityId);

public sealed record ClusterAdministrationBoundEventSchema(
    Guid AggregateId,
    Guid AdministrationId);

public sealed record ClusterAdministrationReleasedEventSchema(
    Guid AggregateId,
    Guid AdministrationId);
