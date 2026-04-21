using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Customer.SegmentationAndLifecycle.ContactPoint;
using Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.ContactPoint;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Customer.SegmentationAndLifecycle.ContactPoint;

/// <summary>
/// E2E smoke test for the business/customer/segmentation-and-lifecycle/contact-point
/// vertical. Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID
/// generation, wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_customer_contact_point.contact_point_read_model</c> table
/// provisioned in Postgres.
///
/// Route segments are hyphenated (<c>contact-point</c>); projection schema/table
/// use underscores (<c>contact_point</c>) per the canonical naming split.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class ContactPointE2ETests
{
    private const string ProjSchema = "projection_business_customer_contact_point";
    private const string ProjTable  = "contact_point_read_model";

    private readonly BusinessE2EFixture _fix;
    public ContactPointE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateActivate_UpdatesProjection_GetReturnsActive()
    {
        var contactPointId = _fix.SeedId("contact-point:happy:id");
        var customerId     = _fix.SeedId("contact-point:happy:customer");
        var corrCreate     = _fix.SeedId("contact-point:happy:corr:create");
        var corrActiv      = _fix.SeedId("contact-point:happy:corr:activate");

        // 1) Create
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/segmentation-and-lifecycle/contact-point/create",
            new CreateContactPointRequestModel(contactPointId, customerId, "Email", "owner@example.com"),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, contactPointId, BusinessE2EConfig.PollTimeout);

        // 2) Activate
        var activateResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/segmentation-and-lifecycle/contact-point/activate",
            new ContactPointIdRequestModel(contactPointId),
            corrActiv);
        Assert.Equal(HttpStatusCode.OK, activateResp.StatusCode);
        var activateAck = await ApiEnvelope.ReadAsync<CommandAck>(activateResp);
        Assert.NotNull(activateAck);
        Assert.True(activateAck!.Success);

        // 3) GET — poll until Status flips to Active.
        ContactPointReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/segmentation-and-lifecycle/contact-point/{contactPointId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<ContactPointReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Active") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(contactPointId, read!.ContactPointId);
        Assert.Equal(customerId, read.CustomerId);
        Assert.Equal("Active", read.Status);
        Assert.NotEqual(default, read.LastUpdatedAt);
    }
}
