using Whycespace.Platform.Api.Core.Contracts;
using Whycespace.Platform.Api.Core.Guards;
using Whycespace.Platform.Api.Core.Services.Feed;
using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Api.Core.Controllers;

/// <summary>
/// WhycePlus event feed and notification controller.
/// READ-ONLY access to event feed projections (CQRS read models).
///
/// PLATFORM GUARDS:
/// - GET only (except POST for mark-as-read on notifications)
/// - No mutation except notification read-status
/// - All data sourced from projections via ProjectionAdapter
/// - Identity required (WhyceId must be present)
/// - No direct engine or domain aggregate access
/// - No Kafka subscription, no event replay
/// - No raw event payloads, internal commands, or policy inputs exposed
///
/// Endpoints:
///   GET  /api/feed?page=1&amp;pageSize=20
///   GET  /api/notifications
///   GET  /api/notifications/unread-count
///   POST /api/notifications/{notificationId}/read
/// </summary>
public sealed class EventFeedController
{
    private readonly IEventFeedService _feedService;

    public EventFeedController(IEventFeedService feedService)
    {
        _feedService = feedService;
    }

    public async Task<ApiResponse> HandleAsync(
        ApiRequest request,
        CancellationToken cancellationToken = default)
    {
        // Guard: Identity required
        if (string.IsNullOrWhiteSpace(request.WhyceId))
            return ApiResponse.Unauthorized(request.TraceId);

        if (!Guid.TryParse(request.WhyceId, out var identityId))
            return ApiResponse.BadRequest("Invalid WhyceId format", request.TraceId);

        var correlationId = request.Headers.GetValueOrDefault("X-Correlation-Id") ?? request.RequestId;

        var segments = request.Endpoint.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // Route: /api/feed
        if (segments.Length >= 2 && string.Equals(segments[1], "feed", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.Equals(request.Method, "GET", StringComparison.OrdinalIgnoreCase))
                return ApiResponse.Forbidden(
                    "Event feed is read-only — only GET requests are permitted", request.TraceId);

            return await HandleFeedQuery(identityId, request, correlationId, cancellationToken);
        }

        // Route: /api/notifications
        if (segments.Length >= 2 && string.Equals(segments[1], "notifications", StringComparison.OrdinalIgnoreCase))
        {
            return await HandleNotificationRoute(identityId, segments, request, correlationId, cancellationToken);
        }

        return ApiResponse.NotFound("Unknown feed endpoint", request.TraceId);
    }

    private async Task<ApiResponse> HandleFeedQuery(
        Guid identityId,
        ApiRequest request,
        string correlationId,
        CancellationToken ct)
    {
        var (page, pageSize) = ParsePagination(request);

        var feed = await _feedService.GetFeedAsync(identityId, page, pageSize, ct);
        return ApiResponse.Ok(
            WhyceResponse.Ok(feed, correlationId, request.TraceId), request.TraceId);
    }

    private async Task<ApiResponse> HandleNotificationRoute(
        Guid identityId,
        string[] segments,
        ApiRequest request,
        string correlationId,
        CancellationToken ct)
    {
        // GET /api/notifications/unread-count
        if (segments.Length >= 3 && string.Equals(segments[2], "unread-count", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.Equals(request.Method, "GET", StringComparison.OrdinalIgnoreCase))
                return ApiResponse.Forbidden(
                    "Unread count is read-only — only GET requests are permitted", request.TraceId);

            var count = await _feedService.GetUnreadCountAsync(identityId, ct);
            return ApiResponse.Ok(
                WhyceResponse.Ok(new { UnreadCount = count }, correlationId, request.TraceId), request.TraceId);
        }

        // POST /api/notifications/{notificationId}/read
        if (segments.Length >= 4 && string.Equals(segments[3], "read", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.Equals(request.Method, "POST", StringComparison.OrdinalIgnoreCase))
                return ApiResponse.BadRequest(
                    "Mark-as-read requires POST method", request.TraceId);

            var notificationId = segments[2];
            await _feedService.MarkAsReadAsync(identityId, notificationId, ct);
            return ApiResponse.Ok(
                WhyceResponse.Ok(new { Marked = true }, correlationId, request.TraceId), request.TraceId);
        }

        // GET /api/notifications
        if (!string.Equals(request.Method, "GET", StringComparison.OrdinalIgnoreCase))
            return ApiResponse.Forbidden(
                "Notifications are read-only — only GET requests are permitted", request.TraceId);

        var notifications = await _feedService.GetNotificationsAsync(identityId, ct);
        return ApiResponse.Ok(
            WhyceResponse.Ok(notifications, correlationId, request.TraceId), request.TraceId);
    }

    private static (int page, int pageSize) ParsePagination(ApiRequest request)
    {
        var page = 1;
        var pageSize = 20;

        if (request.Headers.TryGetValue("X-Page", out var pageStr) && int.TryParse(pageStr, out var parsedPage))
            page = parsedPage;

        if (request.Headers.TryGetValue("X-PageSize", out var sizeStr) && int.TryParse(sizeStr, out var parsedSize))
            pageSize = parsedSize;

        // Also support query string parsing from endpoint
        var queryIndex = request.Endpoint.IndexOf('?');
        if (queryIndex >= 0)
        {
            var query = request.Endpoint[(queryIndex + 1)..];
            foreach (var param in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = param.Split('=', 2);
                if (parts.Length != 2) continue;

                if (string.Equals(parts[0], "page", StringComparison.OrdinalIgnoreCase)
                    && int.TryParse(parts[1], out var qPage))
                    page = qPage;

                if (string.Equals(parts[0], "pageSize", StringComparison.OrdinalIgnoreCase)
                    && int.TryParse(parts[1], out var qSize))
                    pageSize = qSize;
            }
        }

        return (page, pageSize);
    }
}
