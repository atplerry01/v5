using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Binding;

public sealed record BindingReleasedEvent(
    BindingId BindingId,
    Guid AccountId,
    Timestamp ReleasedAt) : DomainEvent;
