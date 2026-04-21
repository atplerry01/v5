using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Customer.IdentityAndProfile.Profile;
using Whycespace.Shared.Contracts.Business.Customer.IdentityAndProfile.Profile;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Customer.IdentityAndProfile.Profile;

/// <summary>
/// E2E smoke test for the business/customer/identity-and-profile/profile vertical.
/// Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID generation,
/// wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_customer_profile.profile_read_model</c> table
/// provisioned in Postgres.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class ProfileE2ETests
{
    private const string ProjSchema = "projection_business_customer_profile";
    private const string ProjTable  = "profile_read_model";

    private readonly BusinessE2EFixture _fix;
    public ProfileE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateActivate_UpdatesProjection_GetReturnsActive()
    {
        var profileId  = _fix.SeedId("profile:happy:id");
        var customerId = _fix.SeedId("profile:happy:customer");
        var corrCreate = _fix.SeedId("profile:happy:corr:create");
        var corrActiv  = _fix.SeedId("profile:happy:corr:activate");

        // 1) Create
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/identity-and-profile/profile/create",
            new CreateProfileRequestModel(profileId, customerId, "Acme Primary Profile"),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, profileId, BusinessE2EConfig.PollTimeout);

        // 2) Activate
        var activateResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/identity-and-profile/profile/activate",
            new ProfileIdRequestModel(profileId),
            corrActiv);
        Assert.Equal(HttpStatusCode.OK, activateResp.StatusCode);
        var activateAck = await ApiEnvelope.ReadAsync<CommandAck>(activateResp);
        Assert.NotNull(activateAck);
        Assert.True(activateAck!.Success);

        // 3) GET — poll until Status flips to Active.
        ProfileReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/identity-and-profile/profile/{profileId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<ProfileReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Active") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(profileId, read!.ProfileId);
        Assert.Equal(customerId, read.CustomerId);
        Assert.Equal("Active", read.Status);
        Assert.NotEqual(default, read.LastUpdatedAt);
    }
}
