using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Binding;

public sealed record CapitalBoundEvent(
    BindingId BindingId,
    Guid AccountId,
    Guid OwnerId,
    OwnershipType OwnershipType,
    Timestamp BoundAt) : DomainEvent;
