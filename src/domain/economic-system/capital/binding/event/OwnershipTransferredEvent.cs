using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Binding;

public sealed record OwnershipTransferredEvent(
    BindingId BindingId,
    Guid PreviousOwnerId,
    Guid NewOwnerId,
    OwnershipType NewOwnershipType,
    Timestamp TransferredAt) : DomainEvent;
