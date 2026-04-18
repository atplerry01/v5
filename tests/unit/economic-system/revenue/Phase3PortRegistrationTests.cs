using Whycespace.Projections.Economic.Capital.Vault;
using Whycespace.Projections.Economic.Revenue.Contract;
using Whycespace.Shared.Contracts.Economic.Revenue.Contract;

namespace Whycespace.Tests.Unit.EconomicSystem.Revenue;

/// <summary>
/// Phase 3.5 T3.5.5 — type-level smoke proof that the Phase 3 read-side
/// resolvers have concrete implementations and that those implementations
/// realize the canonical ports. A pure DI container boot test would require
/// spinning up `EconomicCompositionRoot` with Postgres + Kafka mocks, which
/// is out of scope for unit tests; this assertion catches the most common
/// regression — accidentally deleting a port impl or detaching it from its
/// interface — without that infrastructure cost.
/// </summary>
public sealed class Phase3PortRegistrationTests
{
    [Fact]
    public void ContractStatusGate_ImplementsIContractStatusGate() =>
        Assert.True(typeof(IContractStatusGate).IsAssignableFrom(typeof(ContractStatusGate)),
            $"{nameof(ContractStatusGate)} must implement {nameof(IContractStatusGate)}.");

    [Fact]
    public void ContractAllocationsResolver_ImplementsIContractAllocationsResolver() =>
        Assert.True(typeof(IContractAllocationsResolver).IsAssignableFrom(typeof(ContractAllocationsResolver)),
            $"{nameof(ContractAllocationsResolver)} must implement {nameof(IContractAllocationsResolver)}.");

    [Fact]
    public void PayoutVaultLayoutResolver_ImplementsIPayoutVaultLayoutResolver() =>
        Assert.True(typeof(IPayoutVaultLayoutResolver).IsAssignableFrom(typeof(PayoutVaultLayoutResolver)),
            $"{nameof(PayoutVaultLayoutResolver)} must implement {nameof(IPayoutVaultLayoutResolver)}.");
}
