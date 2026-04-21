using System.Net;
using Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.Allocation;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;
// Disambiguate: Economic.Capital.Allocation also defines CreateAllocationRequestModel
// and AllocationIdRequestModel (global-using via tests/e2e/GlobalUsings.cs).
using CreateAllocationRequestModel = Whycespace.Platform.Api.Controllers.Business.Entitlement.UsageControl.Allocation.CreateAllocationRequestModel;
using AllocationIdRequestModel     = Whycespace.Platform.Api.Controllers.Business.Entitlement.UsageControl.Allocation.AllocationIdRequestModel;

namespace Whycespace.Tests.E2E.Business.Entitlement.UsageControl.Allocation;

/// <summary>
/// E2E smoke test for the business/entitlement/usage-control/allocation
/// vertical. Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID
/// generation, wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_entitlement_allocation.allocation_read_model</c>
/// table provisioned in Postgres.
///
/// Status lifecycle (from AllocationAggregate.Apply + AllocationStatus enum):
/// Pending -> Allocated -> Released.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class AllocationE2ETests
{
    private const string ProjSchema = "projection_business_entitlement_allocation";
    private const string ProjTable  = "allocation_read_model";

    private readonly BusinessE2EFixture _fix;
    public AllocationE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateAllocate_UpdatesProjection_GetReturnsAllocated()
    {
        var allocationId  = _fix.SeedId("allocation:happy:id");
        var resourceId    = _fix.SeedId("allocation:happy:resource");
        var corrCreate    = _fix.SeedId("allocation:happy:corr:create");
        var corrAllocate  = _fix.SeedId("allocation:happy:corr:allocate");

        // 1) Create — RequestedCapacity must be > 0 per aggregate guard.
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/usage-control/allocation/create",
            new CreateAllocationRequestModel(allocationId, resourceId, RequestedCapacity: 10),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, allocationId, BusinessE2EConfig.PollTimeout);

        // 2) Allocate
        var allocateResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/usage-control/allocation/allocate",
            new AllocationIdRequestModel(allocationId),
            corrAllocate);
        Assert.Equal(HttpStatusCode.OK, allocateResp.StatusCode);
        var allocateAck = await ApiEnvelope.ReadAsync<CommandAck>(allocateResp);
        Assert.NotNull(allocateAck);
        Assert.True(allocateAck!.Success);

        // 3) GET — poll until Status flips to Allocated.
        AllocationReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/usage-control/allocation/{allocationId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<AllocationReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Allocated") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(allocationId, read!.AllocationId);
        Assert.Equal(resourceId, read.ResourceId);
        Assert.Equal(10, read.RequestedCapacity);
        Assert.Equal("Allocated", read.Status);
        Assert.NotEqual(default, read.CreatedAt);
    }
}
