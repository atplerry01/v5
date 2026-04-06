using Whycespace.Shared.Primitives.Time;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Domain.EconomicSystem.Ledger.Ledger;
using Whycespace.Domain.EconomicSystem.Ledger.Settlement;
using Whycespace.Domain.EconomicSystem.Ledger.Treasury;
using DomainMoney = Whycespace.Domain.SharedKernel.Primitive.Money.Money;
using DomainCurrency = Whycespace.Domain.SharedKernel.Primitive.Money.Currency;
using DomainAmount = Whycespace.Domain.SharedKernel.Primitive.Money.Amount;
using Whycespace.Shared.Primitives.Money;
using Whycespace.Runtime.Engine.Domain;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Domain.Economic.Ledger;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Policy;
using LedgerEntryType = Whycespace.Domain.EconomicSystem.Ledger.Ledger.EntryType;

namespace Whycespace.Runtime.Engine.Domain.Economic.Ledger;

public sealed class LedgerDomainService : GovernedDomainServiceBase, ILedgerDomainService
{
    private readonly IAggregateStore _aggregateStore;
    private readonly IIdGenerator _idGenerator;
    private readonly SettlementInvariantService _settlementInvariantService = new();

    public LedgerDomainService(
        IAggregateStore aggregateStore,
        IPolicyEvaluator policyEvaluator,
        IPolicyDecisionAnchor chainAnchor,
        EnforcementMetrics metrics,
        EnforcementAnomalyEmitter anomalyEmitter,
        IClock clock,
        IIdGenerator idGenerator)
        : base(policyEvaluator, chainAnchor, metrics, anomalyEmitter, clock)
    {
        _aggregateStore = aggregateStore;
        _idGenerator = idGenerator;
    }

    public async Task<DomainOperationResult> RecordDoubleEntryAsync(DomainExecutionContext context, string ledgerId, string entryId, string accountCode, string accountName, decimal debitAmount, decimal creditAmount, string currencyCode)
    {
        var transactionAmount = Math.Max(debitAmount, creditAmount);
        var currency = new Whycespace.Shared.Primitives.Money.Currency(currencyCode);

        var policyInput = new LedgerPolicyInput
        {
            IdentityId = context.ActorId,
            TransactionId = Guid.Parse(entryId),
            Amount = new Money(transactionAmount, currency),
            Accounts = new[] { Guid.Parse(ledgerId) },
            Jurisdiction = context.Headers.GetValueOrDefault("X-Jurisdiction") ?? throw new InvalidOperationException("Jurisdiction header is required for ledger policy evaluation")
        };

        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<LedgerAggregate>(ledgerId);

            aggregate.RecordDoubleEntry(
                Guid.Parse(entryId),
                new LedgerAccount(accountCode, accountName),
                new DebitAmount(new DomainAmount(debitAmount)),
                new CreditAmount(new DomainAmount(creditAmount)),
                new DomainCurrency(currencyCode));

            await _aggregateStore.SaveAsync(aggregate);

            return ((Guid?)Guid.Parse(ledgerId), (object?)new
            {
                LedgerId = ledgerId,
                EntryId = entryId,
                AccountCode = accountCode,
                DebitAmount = debitAmount,
                CreditAmount = creditAmount,
                CurrencyCode = currencyCode
            });
        }, policyInput);
    }

    public async Task<DomainOperationResult> CreateSettlementAsync(DomainExecutionContext context, string id, string payeeIdentityId, Money amount)
    {
        var policyInput = new LedgerPolicyInput
        {
            IdentityId = context.ActorId,
            TransactionId = Guid.Parse(id),
            Amount = amount,
            Accounts = new[] { Guid.Parse(id), Guid.Parse(payeeIdentityId) },
            Jurisdiction = context.Headers.GetValueOrDefault("X-Jurisdiction") ?? throw new InvalidOperationException("Jurisdiction header is required for ledger policy evaluation")
        };

        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var domainAmount = new DomainMoney(amount.Amount, new DomainCurrency(amount.Currency.Code, amount.Currency.Name, amount.Currency.DecimalPlaces));
            var aggregate = SettlementAggregate.Execute(
                Guid.Parse(id),
                Guid.Parse(payeeIdentityId),
                domainAmount);
            await _aggregateStore.SaveAsync(aggregate);
            return ((Guid?)Guid.Parse(id), (object?)null);
        }, policyInput);
    }

    public async Task<DomainOperationResult> ExecuteSettlementAsync(
        DomainExecutionContext context,
        string settlementId,
        Guid transactionId,
        IReadOnlyList<LedgerEntryDto> ledgerEntries,
        Money amount)
    {
        var policyInput = new SettlementPolicyInput
        {
            IdentityId = context.ActorId,
            TransactionId = transactionId,
            Amount = amount,
            Accounts = new[] { Guid.Parse(settlementId) },
            Jurisdiction = context.Headers.GetValueOrDefault("X-Jurisdiction") ?? throw new InvalidOperationException("Jurisdiction header is required for settlement policy evaluation")
        };

        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<SettlementAggregate>(settlementId);

            var domainEntries = new List<LedgerEntry>(ledgerEntries.Count);
            for (var i = 0; i < ledgerEntries.Count; i++)
            {
                var dto = ledgerEntries[i];
                var entryType = dto.EntryType == "Debit" ? LedgerEntryType.Debit : LedgerEntryType.Credit;
                domainEntries.Add(LedgerEntry.Create(
                    new LedgerEntryId(_idGenerator.DeterministicGuid(settlementId, transactionId.ToString(), i.ToString())),
                    dto.AccountId,
                    new DomainAmount(dto.Amount),
                    entryType,
                    dto.TransactionId));
            }

            var domainAmount = new DomainMoney(amount.Amount, new DomainCurrency(amount.Currency.Code, amount.Currency.Name, amount.Currency.DecimalPlaces));

            aggregate.ExecuteSettlement(
                transactionId,
                domainEntries,
                domainAmount,
                _settlementInvariantService);

            await _aggregateStore.SaveAsync(aggregate);

            return ((Guid?)Guid.Parse(settlementId), (object?)new
            {
                SettlementId = settlementId,
                TransactionId = transactionId,
                Amount = amount.Amount,
                CurrencyCode = amount.Currency.Code
            });
        }, policyInput);
    }

    public async Task<DomainOperationResult> CreateTreasuryAsync(DomainExecutionContext context, string id)
    {
        var policyInput = new LedgerPolicyInput
        {
            IdentityId = context.ActorId,
            TransactionId = Guid.Parse(id),
            Amount = Money.Zero(Whycespace.Shared.Primitives.Money.Currency.USD),
            Accounts = new[] { Guid.Parse(id) },
            Jurisdiction = context.Headers.GetValueOrDefault("X-Jurisdiction") ?? throw new InvalidOperationException("Jurisdiction header is required for ledger policy evaluation")
        };

        return await ExecuteGovernedUntypedAsync(context, async () =>
        {
            var aggregate = await _aggregateStore.LoadAsync<TreasuryAggregate>(id);
            aggregate.Create(Guid.Parse(id));
            await _aggregateStore.SaveAsync(aggregate);
            return ((Guid?)Guid.Parse(id), (object?)null);
        }, policyInput);
    }
}
