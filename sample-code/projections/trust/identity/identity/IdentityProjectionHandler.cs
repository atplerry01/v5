using Whycespace.Projections.Identity;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Trust.Identity.Identity;

public sealed class IdentityProjectionHandler
{
    public string ProjectionName => "whyce.trust.identity.identity";

    public string[] EventTypes =>
    [
        "whyce.trust.identity.identity.created",
        "whyce.trust.identity.identity.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IIdentityViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new IdentityReadModel
        {
            IdentityId = @event.CorrelationId,
            IdentityType = "user",
            DisplayName = "projected-identity",
            Status = "Active",
            CreatedAt = @event.Timestamp
        };

        await repository.SaveAsync(model, ct);
    }
}
