using Whycespace.Platform.Adapters;
using Whycespace.Platform.Api.Core.Contracts.Feed;

namespace Whycespace.Platform.Api.Core.Services.Feed;

/// <summary>
/// Projection-backed event feed query service.
/// Delegates all queries to ProjectionAdapter -> IProjectionQuerySource.
/// Pure read-only mapping — no business logic, no Kafka, no event replay.
///
/// The ONLY mutation allowed is marking a notification as read,
/// which is scoped to notification display state only.
///
/// Projection names follow the convention: "feed.{entity}" or "notification.{entity}".
/// </summary>
public sealed class EventFeedService : IEventFeedService
{
    private const int MaxPageSize = 50;

    private readonly ProjectionAdapter _projections;

    public EventFeedService(ProjectionAdapter projections)
    {
        _projections = projections;
    }

    public async Task<IReadOnlyList<EventFeedItemView>> GetFeedAsync(
        Guid identityId, int page, int pageSize, CancellationToken ct)
    {
        var clampedPageSize = Math.Clamp(pageSize, 1, MaxPageSize);
        var clampedPage = Math.Max(1, page);

        var response = await _projections.QueryListAsync<EventFeedItemView>(
            "feed.events.by-identity",
            new Dictionary<string, object>
            {
                ["identityId"] = identityId,
                ["page"] = clampedPage,
                ["pageSize"] = clampedPageSize
            },
            cancellationToken: ct);

        return ExtractList<EventFeedItemView>(response);
    }

    public async Task<IReadOnlyList<NotificationView>> GetNotificationsAsync(
        Guid identityId, CancellationToken ct)
    {
        var response = await _projections.QueryListAsync<NotificationView>(
            "notification.by-identity",
            new Dictionary<string, object>
            {
                ["identityId"] = identityId
            },
            cancellationToken: ct);

        return ExtractList<NotificationView>(response);
    }

    public async Task<int> GetUnreadCountAsync(
        Guid identityId, CancellationToken ct)
    {
        var response = await _projections.QueryAsync<int>(
            "notification.unread-count",
            new Dictionary<string, object>
            {
                ["identityId"] = identityId
            },
            cancellationToken: ct);

        return response.Data is int count ? count : 0;
    }

    public async Task MarkAsReadAsync(
        Guid identityId, string notificationId, CancellationToken ct)
    {
        // This is the ONLY allowed mutation — scoped to notification read-status only.
        // Delegates to projection adapter which updates the read-model state.
        await _projections.QueryAsync<object>(
            "notification.mark-read",
            new Dictionary<string, object>
            {
                ["identityId"] = identityId,
                ["notificationId"] = notificationId
            },
            cancellationToken: ct);
    }

    private static IReadOnlyList<T> ExtractList<T>(Whycespace.Platform.Middleware.ApiResponse response)
    {
        if (response.StatusCode is < 200 or >= 300)
            return [];

        return response.Data as IReadOnlyList<T> ?? [];
    }
}
