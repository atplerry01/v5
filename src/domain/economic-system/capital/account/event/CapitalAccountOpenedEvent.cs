using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Account;

public sealed record CapitalAccountOpenedEvent(
    AccountId AccountId,
    OwnerId OwnerId,
    Currency Currency,
    Timestamp CreatedAt) : DomainEvent;
