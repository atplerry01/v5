using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Customer.IdentityAndProfile.Customer;
using Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Customer;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Customer.IdentityAndProfile.Customer;

/// <summary>
/// E2E smoke test for the business/customer/identity-and-profile/customer vertical.
/// Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID generation,
/// wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_customer_customer.customer_read_model</c> table
/// provisioned in Postgres.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class CustomerE2ETests
{
    private const string ProjSchema = "projection_business_customer_customer";
    private const string ProjTable  = "customer_read_model";

    private readonly BusinessE2EFixture _fix;
    public CustomerE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateActivate_UpdatesProjection_GetReturnsActive()
    {
        var customerId = _fix.SeedId("customer:happy:id");
        var corrCreate = _fix.SeedId("customer:happy:corr:create");
        var corrActiv  = _fix.SeedId("customer:happy:corr:activate");

        // 1) Create
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/identity-and-profile/customer/create",
            new CreateCustomerRequestModel(customerId, "Acme Corp", "Organization", "ACME-001"),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, customerId, BusinessE2EConfig.PollTimeout);

        // 2) Activate
        var activateResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/identity-and-profile/customer/activate",
            new CustomerIdRequestModel(customerId),
            corrActiv);
        Assert.Equal(HttpStatusCode.OK, activateResp.StatusCode);
        var activateAck = await ApiEnvelope.ReadAsync<CommandAck>(activateResp);
        Assert.NotNull(activateAck);
        Assert.True(activateAck!.Success);

        // 3) GET — poll until Status flips to Active.
        CustomerReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/identity-and-profile/customer/{customerId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<CustomerReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Active") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(customerId, read!.CustomerId);
        Assert.Equal("Active", read.Status);
        Assert.NotEqual(default, read.LastUpdatedAt);
    }
}
