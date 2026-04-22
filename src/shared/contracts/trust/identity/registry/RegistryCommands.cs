using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Trust.Identity.Registry;

public sealed record InitiateRegistrationCommand(
    Guid RegistryId,
    string Email,
    string RegistrationType,
    DateTimeOffset InitiatedAt) : IHasAggregateId
{
    public Guid AggregateId => RegistryId;
}

public sealed record VerifyRegistrationCommand(Guid RegistryId) : IHasAggregateId
{
    public Guid AggregateId => RegistryId;
}

public sealed record ActivateRegistrationCommand(Guid RegistryId) : IHasAggregateId
{
    public Guid AggregateId => RegistryId;
}

public sealed record RejectRegistrationCommand(Guid RegistryId, string Reason) : IHasAggregateId
{
    public Guid AggregateId => RegistryId;
}

public sealed record LockRegistrationCommand(Guid RegistryId, string Reason) : IHasAggregateId
{
    public Guid AggregateId => RegistryId;
}
