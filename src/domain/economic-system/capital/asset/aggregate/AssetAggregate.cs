using Whycespace.Domain.EconomicSystem.Capital.Account;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Asset;

public sealed class AssetAggregate : AggregateRoot
{
    public AssetId AssetId { get; private set; }
    public OwnerId OwnerId { get; private set; }
    public Amount Value { get; private set; }
    public Currency Currency { get; private set; }
    public AssetStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }
    public Timestamp LastValuedAt { get; private set; }

    private AssetAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static AssetAggregate Create(
        AssetId assetId,
        OwnerId ownerId,
        Amount initialValue,
        Currency currency,
        Timestamp createdAt)
    {
        Guard.Against(initialValue.Value <= 0m, "Initial value must be greater than zero.");

        var aggregate = new AssetAggregate();

        aggregate.RaiseDomainEvent(new AssetCreatedEvent(
            assetId,
            ownerId.Value,
            initialValue,
            currency,
            createdAt));

        return aggregate;
    }

    // D-ID-REF-01 dual-path: legacy Guid overload normalizes to typed ref.
    public static AssetAggregate Create(
        AssetId assetId,
        Guid ownerId,
        Amount initialValue,
        Currency currency,
        Timestamp createdAt)
        => Create(assetId, new OwnerId(ownerId), initialValue, currency, createdAt);

    // ── Behavior ─────────────────────────────────────────────────

    public void Revalue(Amount newValue, Timestamp valuedAt)
    {
        if (Status == AssetStatus.Disposed)
            throw AssetErrors.CannotValueDisposedAsset();

        Guard.Against(newValue.Value <= 0m, "New value must be greater than zero.");

        RaiseDomainEvent(new AssetValuedEvent(
            AssetId,
            Value,
            newValue,
            Currency,
            valuedAt));
    }

    public void Dispose(Timestamp disposedAt)
    {
        if (Status == AssetStatus.Disposed)
            throw AssetErrors.AssetAlreadyDisposed();

        RaiseDomainEvent(new AssetDisposedEvent(
            AssetId,
            Value,
            disposedAt));
    }

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case AssetCreatedEvent e:
                AssetId = e.AssetId;
                OwnerId = new OwnerId(e.OwnerId);
                Value = e.InitialValue;
                Currency = e.Currency;
                Status = AssetStatus.Active;
                CreatedAt = e.CreatedAt;
                LastValuedAt = e.CreatedAt;
                break;

            case AssetValuedEvent e:
                Value = e.NewValue;
                Status = AssetStatus.Valued;
                LastValuedAt = e.ValuedAt;
                break;

            case AssetDisposedEvent:
                Status = AssetStatus.Disposed;
                break;
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (Value.Value < 0m)
            throw AssetErrors.NegativeAssetValue();

        if (Status != AssetStatus.Disposed)
        {
            Guard.Against(OwnerId.Value == Guid.Empty, "OwnerId must not be empty for a non-disposed asset.");
        }
    }
}
