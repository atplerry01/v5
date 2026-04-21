using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Structural.Cluster.Administration;

public sealed record EstablishAdministrationCommand(
    Guid AdministrationId,
    Guid ClusterReference,
    string AdministrationName) : IHasAggregateId
{
    public Guid AggregateId => AdministrationId;
}

public sealed record EstablishAdministrationWithParentCommand(
    Guid AdministrationId,
    Guid ClusterReference,
    string AdministrationName,
    DateTimeOffset EffectiveAt) : IHasAggregateId
{
    public Guid AggregateId => AdministrationId;
}

public sealed record ActivateAdministrationCommand(
    Guid AdministrationId) : IHasAggregateId
{
    public Guid AggregateId => AdministrationId;
}

public sealed record SuspendAdministrationCommand(
    Guid AdministrationId) : IHasAggregateId
{
    public Guid AggregateId => AdministrationId;
}

public sealed record RetireAdministrationCommand(
    Guid AdministrationId) : IHasAggregateId
{
    public Guid AggregateId => AdministrationId;
}
