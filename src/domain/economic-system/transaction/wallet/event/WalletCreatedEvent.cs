using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Wallet;

public sealed record WalletCreatedEvent(
    WalletId WalletId,
    Guid OwnerId,
    Guid AccountId,
    Timestamp CreatedAt) : DomainEvent;
