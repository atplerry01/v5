using Whycespace.Domain.EconomicSystem.Capital.Account;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Vault;

public sealed class VaultAggregate : AggregateRoot
{
    private readonly List<VaultSlice> _slices = new();

    public VaultId VaultId { get; private set; }
    public OwnerId OwnerId { get; private set; }
    public Amount TotalStored { get; private set; }
    public Currency Currency { get; private set; }
    public IReadOnlyList<VaultSlice> Slices => _slices.AsReadOnly();

    private VaultAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static VaultAggregate Create(
        VaultId vaultId,
        OwnerId ownerId,
        Currency currency,
        Timestamp createdAt)
    {
        var aggregate = new VaultAggregate();
        aggregate.RaiseDomainEvent(new VaultCreatedEvent(vaultId, ownerId.Value, currency, createdAt));
        return aggregate;
    }

    // D-ID-REF-01 dual-path: legacy Guid overload normalizes to typed ref.
    public static VaultAggregate Create(
        VaultId vaultId,
        Guid ownerId,
        Currency currency,
        Timestamp createdAt)
        => Create(vaultId, new OwnerId(ownerId), currency, createdAt);

    // ── Slice Management ─────────────────────────────────────────

    public void AddSlice(SliceId sliceId, Amount totalCapacity, Currency currency)
    {
        if (totalCapacity.Value <= 0m)
            throw VaultErrors.InvalidAmount();

        if (currency != Currency)
            throw VaultErrors.CurrencyMismatch(Currency, currency);

        if (_slices.Exists(s => s.Id == sliceId))
            throw VaultErrors.DuplicateSliceId(sliceId);

        RaiseDomainEvent(new VaultSliceCreatedEvent(VaultId, sliceId, totalCapacity, currency));
    }

    // ── Deposit ──────────────────────────────────────────────────

    public void DepositToSlice(SliceId sliceId, Amount amount)
    {
        if (amount.Value <= 0m)
            throw VaultErrors.InvalidAmount();

        var slice = FindSliceOrThrow(sliceId);

        if (slice.Status == SliceStatus.Closed)
            throw VaultErrors.InvalidSliceState(sliceId, slice.Status);

        var newSliceCapacity = new Amount(slice.TotalCapacity.Value + amount.Value);
        var newVaultTotal = new Amount(TotalStored.Value + amount.Value);

        RaiseDomainEvent(new CapitalDepositedEvent(
            VaultId, sliceId, amount, newSliceCapacity, newVaultTotal));
    }

    // ── Allocate ─────────────────────────────────────────────────

    public void AllocateFromSlice(SliceId sliceId, Amount amount)
    {
        if (amount.Value <= 0m)
            throw VaultErrors.InvalidAmount();

        var slice = FindSliceOrThrow(sliceId);

        if (slice.Status == SliceStatus.Closed)
            throw VaultErrors.InvalidSliceState(sliceId, slice.Status);

        if (amount.Value > slice.AvailableAmount.Value)
            throw VaultErrors.SliceCapacityExceeded(sliceId, amount, slice.AvailableAmount);

        var newAvailable = new Amount(slice.AvailableAmount.Value - amount.Value);
        var newUsed = new Amount(slice.UsedAmount.Value + amount.Value);

        RaiseDomainEvent(new CapitalAllocatedToSliceEvent(
            VaultId, sliceId, amount, newAvailable, newUsed));
    }

    // ── Release ──────────────────────────────────────────────────

    public void ReleaseToSlice(SliceId sliceId, Amount amount)
    {
        if (amount.Value <= 0m)
            throw VaultErrors.InvalidAmount();

        var slice = FindSliceOrThrow(sliceId);

        if (slice.Status == SliceStatus.Closed)
            throw VaultErrors.InvalidSliceState(sliceId, slice.Status);

        if (amount.Value > slice.UsedAmount.Value)
            throw VaultErrors.InsufficientSliceAllocation(sliceId, amount, slice.UsedAmount);

        var newAvailable = new Amount(slice.AvailableAmount.Value + amount.Value);
        var newUsed = new Amount(slice.UsedAmount.Value - amount.Value);

        RaiseDomainEvent(new CapitalReleasedFromSliceEvent(
            VaultId, sliceId, amount, newAvailable, newUsed));
    }

    // ── Withdraw ─────────────────────────────────────────────────

    public void WithdrawFromSlice(SliceId sliceId, Amount amount)
    {
        if (amount.Value <= 0m)
            throw VaultErrors.InvalidAmount();

        var slice = FindSliceOrThrow(sliceId);

        if (slice.Status == SliceStatus.Closed)
            throw VaultErrors.InvalidSliceState(sliceId, slice.Status);

        if (amount.Value > slice.AvailableAmount.Value)
            throw VaultErrors.InsufficientSliceCapacity(sliceId, amount, slice.AvailableAmount);

        var newSliceCapacity = new Amount(slice.TotalCapacity.Value - amount.Value);
        var newVaultTotal = new Amount(TotalStored.Value - amount.Value);

        RaiseDomainEvent(new CapitalWithdrawnEvent(
            VaultId, sliceId, amount, newSliceCapacity, newVaultTotal));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case VaultCreatedEvent e:
                VaultId = e.VaultId;
                OwnerId = new OwnerId(e.OwnerId);
                Currency = e.Currency;
                TotalStored = new Amount(0m);
                break;

            case VaultSliceCreatedEvent e:
                var newSlice = VaultSlice.Create(e.SliceId, e.VaultId, e.TotalCapacity, e.Currency);
                _slices.Add(newSlice);
                TotalStored = new Amount(TotalStored.Value + e.TotalCapacity.Value);
                break;

            case CapitalDepositedEvent e:
                FindSlice(e.SliceId)?.Deposit(e.DepositedAmount);
                TotalStored = e.NewVaultTotal;
                break;

            case CapitalAllocatedToSliceEvent e:
                FindSlice(e.SliceId)?.Allocate(e.AllocatedAmount);
                break;

            case CapitalReleasedFromSliceEvent e:
                FindSlice(e.SliceId)?.Release(e.ReleasedAmount);
                break;

            case CapitalWithdrawnEvent e:
                FindSlice(e.SliceId)?.Withdraw(e.WithdrawnAmount);
                TotalStored = e.NewVaultTotal;
                break;
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (TotalStored.Value < 0m)
            throw VaultErrors.NegativeVaultBalance();

        var sliceCapacitySum = 0m;
        foreach (var slice in _slices)
        {
            slice.EnsureSliceInvariants();
            sliceCapacitySum += slice.TotalCapacity.Value;
        }

        if (sliceCapacitySum != TotalStored.Value)
            throw VaultErrors.VaultTotalMismatch(TotalStored, new Amount(sliceCapacitySum));
    }

    // ── Helpers ──────────────────────────────────────────────────

    private VaultSlice FindSliceOrThrow(SliceId sliceId)
    {
        return FindSlice(sliceId)
            ?? throw VaultErrors.SliceNotFound(sliceId);
    }

    private VaultSlice? FindSlice(SliceId sliceId)
    {
        return _slices.Find(s => s.Id == sliceId);
    }
}
