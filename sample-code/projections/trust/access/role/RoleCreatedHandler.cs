using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Identity;

public static class RoleCreatedHandler
{
    public static async Task HandleAsync(ProjectionEvent @event, IdentityReadStore store, CancellationToken ct)
    {
        var json = IdentityRegisteredHandler.ParsePayload(@event);
        var roleId = json.GetStringOrNull("RoleId");
        if (roleId is null) return;

        // Idempotency: skip if role already projected
        var existing = await store.GetRoleAsync(roleId, ct);
        if (existing is not null) return;

        var model = new IdentityRoleReadModel
        {
            RoleId = roleId,
            Name = json.GetStringOrNull("Name") ?? "",
            Scope = json.GetStringOrNull("Scope") ?? "",
            Status = "Active",
            CreatedAt = @event.Timestamp
        };

        await store.SetRoleAsync(roleId, model, ct);

        await IdentityRegisteredHandler.UpdateMetrics(store, @event.Timestamp, m => m with
        {
            TotalRoles = m.TotalRoles + 1,
            LastUpdated = @event.Timestamp
        }, ct);
    }
}
