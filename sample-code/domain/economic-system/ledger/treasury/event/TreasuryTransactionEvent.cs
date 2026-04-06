using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Treasury;

public sealed record TreasuryTransactionEvent(Guid TransactionId, Guid AuthorizedByIdentityId) : DomainEvent;
