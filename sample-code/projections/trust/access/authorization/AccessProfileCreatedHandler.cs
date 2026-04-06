using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Identity;

public static class AccessProfileCreatedHandler
{
    public static async Task HandleAsync(ProjectionEvent @event, IdentityReadStore store, CancellationToken ct)
    {
        var json = IdentityRegisteredHandler.ParsePayload(@event);
        var profileId = json.GetStringOrNull("ProfileId");
        var identityId = json.GetStringOrNull("IdentityId");
        if (profileId is null || identityId is null) return;

        // Idempotency: skip if access profile already projected
        var existing = await store.GetAccessProfileAsync(profileId, ct);
        if (existing is not null) return;

        var model = new IdentityAccessProfileReadModel
        {
            ProfileId = profileId,
            IdentityId = identityId,
            AccessLevel = json.GetStringOrNull("AccessLevel") ?? "Basic",
            Status = "Active",
            CreatedAt = @event.Timestamp
        };

        await store.SetAccessProfileAsync(profileId, model, ct);
    }
}
