using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Wallet;

public sealed record TransactionRequestedEvent(
    WalletId WalletId,
    Guid AccountId,
    Guid DestinationAccountId,
    Amount Amount,
    Currency Currency,
    Timestamp RequestedAt) : DomainEvent;
