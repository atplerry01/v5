using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Order.OrderCore.Reservation;
using Whycespace.Shared.Contracts.Business.Order.OrderCore.Reservation;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Order.OrderCore.Reservation;

/// <summary>
/// E2E smoke test for the business/order/order-core/reservation vertical.
/// Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID generation,
/// wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_order_reservation.reservation_read_model</c> table
/// provisioned in Postgres.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class ReservationE2ETests
{
    private const string ProjSchema = "projection_business_order_reservation";
    private const string ProjTable  = "reservation_read_model";

    private readonly BusinessE2EFixture _fix;
    public ReservationE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_HoldConfirm_UpdatesProjection_GetReturnsConfirmed()
    {
        var reservationId = _fix.SeedId("reservation:happy:id");
        var orderId       = _fix.SeedId("reservation:happy:order");
        var lineItemId    = _fix.SeedId("reservation:happy:lineitem");
        var subjectId     = _fix.SeedId("reservation:happy:subject");
        var corrHold      = _fix.SeedId("reservation:happy:corr:hold");
        var corrConfirm   = _fix.SeedId("reservation:happy:corr:confirm");

        const int subjectKind = 1;
        const decimal quantityValue = 5m;
        const string quantityUnit = "unit";
        var expiresAt = _fix.Clock.UtcNow + TimeSpan.FromHours(1);

        // 1) Hold
        var holdResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/order-core/reservation/hold",
            new HoldReservationRequestModel(
                reservationId,
                orderId,
                lineItemId,
                subjectKind,
                subjectId,
                quantityValue,
                quantityUnit,
                expiresAt),
            corrHold);
        Assert.Equal(HttpStatusCode.OK, holdResp.StatusCode);
        var holdAck = await ApiEnvelope.ReadAsync<CommandAck>(holdResp);
        Assert.NotNull(holdAck);
        Assert.True(holdAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, reservationId, BusinessE2EConfig.PollTimeout);

        // 2) Confirm
        var confirmResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/order-core/reservation/confirm",
            new ReservationIdRequestModel(reservationId),
            corrConfirm);
        Assert.Equal(HttpStatusCode.OK, confirmResp.StatusCode);
        var confirmAck = await ApiEnvelope.ReadAsync<CommandAck>(confirmResp);
        Assert.NotNull(confirmAck);
        Assert.True(confirmAck!.Success);

        // 3) GET projection — poll until Status == "Confirmed".
        ReservationReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/order-core/reservation/{reservationId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<ReservationReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Confirmed") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(reservationId, read!.ReservationId);
        Assert.Equal(orderId, read.OrderId);
        Assert.Equal("Confirmed", read.Status);
        Assert.NotEqual(default, read.HeldAt);
    }
}
