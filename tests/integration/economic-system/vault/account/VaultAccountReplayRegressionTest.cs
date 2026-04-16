using System.Text.Json;
using Whycespace.Domain.EconomicSystem.Subject.Subject;
using Whycespace.Domain.EconomicSystem.Vault.Account;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.EventFabric.DomainSchemas;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Integration.EconomicSystem.Vault.Account;

// D10 regression: VaultAccountAggregate must round-trip Currency through the
// event-store replay path. The store persists the schema-mapped JSONB
// (flat primitives); the deserializer rehydrates into the DOMAIN event type
// which uses value-object wrappers. Currency must come back as Currency("USD"),
// not Currency("").
public sealed class VaultAccountReplayRegressionTest
{
    private static readonly TestIdGenerator IdGen = new();

    [Fact]
    public void VaultAccountCreated_RoundTrip_PreservesCurrency()
    {
        // 1. Build the registry the same way the host does for vault.
        var registry = new EventSchemaRegistry();
        DomainSchemaCatalog.RegisterEconomic(registry);
        var deserializer = new EventDeserializer(registry);

        // 2. Construct the aggregate via the factory.
        var vaultAccountId = new VaultAccountId(IdGen.Generate("D10:Vault"));
        var ownerSubjectId = new SubjectId(IdGen.Generate("D10:Owner"));
        var currency = new Currency("USD");
        var aggregate = VaultAccountAggregate.Create(vaultAccountId, ownerSubjectId, currency);

        var seedEvent = aggregate.DomainEvents.OfType<VaultAccountCreatedEvent>().Single();

        // 3. Mirror the event-store write path: schema-map then JSON-serialize.
        var mappedPayload = registry.MapPayload(nameof(VaultAccountCreatedEvent), seedEvent);
        var jsonb = JsonSerializer.Serialize(mappedPayload, mappedPayload.GetType());

        // 4. Read it back the same way LoadEventsAsync does.
        var rehydrated = deserializer.DeserializeStored(nameof(VaultAccountCreatedEvent), jsonb);
        var typed = Assert.IsType<VaultAccountCreatedEvent>(rehydrated);

        // 5. Replay onto a fresh aggregate (mirrors LoadFromHistory).
        var replayed = (VaultAccountAggregate)Activator.CreateInstance(
            typeof(VaultAccountAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(new object[] { typed });

        // The smoking gun: Currency.Code must be "USD", not "".
        Assert.Equal("USD", replayed.Currency.Code);
        Assert.Equal(vaultAccountId, replayed.VaultAccountId);
        Assert.Equal(ownerSubjectId, replayed.OwnerSubjectId);
    }
}
