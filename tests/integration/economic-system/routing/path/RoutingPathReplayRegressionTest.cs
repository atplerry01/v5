using Whycespace.Domain.EconomicSystem.Routing.Path;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.EventFabric.DomainSchemas;
using Whycespace.Tests.Integration.Setup;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Integration.EconomicSystem.Routing.Path;

// β #2 / D10 generalization: routing/path was identified as having a LATENT
// replay defect (the prior delta report noted that PathId reconstructed as
// Guid.Empty on replay because Activate doesn't read it). This regression
// asserts that the WrappedPrimitiveValueObjectConverterFactory now closes
// the gap end-to-end: the replayed aggregate's PathId equals the seed value.
public sealed class RoutingPathReplayRegressionTest
{
    private static readonly TestIdGenerator IdGen = new();

    [Fact]
    public void RoutingPathDefined_RoundTrip_PreservesPathIdAndType()
    {
        var registry = new EventSchemaRegistry();
        DomainSchemaCatalog.RegisterEconomic(registry);

        var pathId = new PathId(IdGen.Generate("Replay:RoutingPath:1"));
        var seed = RoutingPathAggregate.DefinePath(
            pathId,
            PathType.Internal,
            "USD->EUR direct",
            priority: 1);

        AggregateReplayHarness.VerifyRoundTrip(seed, registry, replayed =>
        {
            Assert.Equal(pathId, replayed.PathId);
            Assert.Equal(PathType.Internal, replayed.PathType);
            Assert.Equal("USD->EUR direct", replayed.Conditions);
            Assert.Equal(1, replayed.Priority);
        });
    }
}
