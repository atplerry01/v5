using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Whycespace.Shared.Contracts.Business.Order.OrderCore.Reservation;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Api.Controllers.Business.Order.OrderCore.Reservation;

[Authorize]
[ApiController]
[Route("api/order-core/reservation")]
[ApiExplorerSettings(GroupName = "business.order.order-core.reservation")]
public sealed class ReservationController : BusinessControllerBase
{
    private static readonly DomainRoute ReservationRoute = new("business", "order", "reservation");

    public ReservationController(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        IConfiguration configuration)
        : base(dispatcher, idGenerator, clock, configuration) { }

    [HttpPost("hold")]
    public Task<IActionResult> Hold([FromBody] ApiRequest<HoldReservationRequestModel> request, CancellationToken ct)
    {
        var p = request.Data;
        // HeldAt is read from IClock (host-bound wall clock in production;
        // TestClock in tests) so the value flows through the event-sourced
        // invariant without leaking DateTimeOffset.UtcNow into controller code.
        var cmd = new HoldReservationCommand(
            p.ReservationId,
            p.OrderId,
            p.LineItemId,
            p.SubjectKind,
            p.SubjectId,
            p.QuantityValue,
            p.QuantityUnit,
            p.ExpiresAt,
            Clock.UtcNow);
        return Dispatch(cmd, ReservationRoute, "reservation_held", "business.order.reservation.hold_failed", ct);
    }

    [HttpPost("confirm")]
    public Task<IActionResult> Confirm([FromBody] ApiRequest<ReservationIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ConfirmReservationCommand(request.Data.ReservationId, Clock.UtcNow);
        return Dispatch(cmd, ReservationRoute, "reservation_confirmed", "business.order.reservation.confirm_failed", ct);
    }

    [HttpPost("release")]
    public Task<IActionResult> Release([FromBody] ApiRequest<ReservationIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ReleaseReservationCommand(request.Data.ReservationId, Clock.UtcNow);
        return Dispatch(cmd, ReservationRoute, "reservation_released", "business.order.reservation.release_failed", ct);
    }

    [HttpPost("expire")]
    public Task<IActionResult> Expire([FromBody] ApiRequest<ReservationIdRequestModel> request, CancellationToken ct)
    {
        var cmd = new ExpireReservationCommand(request.Data.ReservationId, Clock.UtcNow);
        return Dispatch(cmd, ReservationRoute, "reservation_expired", "business.order.reservation.expire_failed", ct);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetReservation(Guid id, CancellationToken ct) =>
        LoadReadModel<ReservationReadModel>(
            id,
            "projection_business_order_reservation",
            "reservation_read_model",
            "business.order.reservation.not_found",
            ct);
}

public sealed record HoldReservationRequestModel(
    Guid ReservationId,
    Guid OrderId,
    Guid? LineItemId,
    int SubjectKind,
    Guid SubjectId,
    decimal QuantityValue,
    string QuantityUnit,
    DateTimeOffset ExpiresAt);

public sealed record ReservationIdRequestModel(Guid ReservationId);
