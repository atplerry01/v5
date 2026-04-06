using Whycespace.Platform.Api.Core.Contracts.Access;

namespace Whycespace.Platform.Api.Core.Services.Access;

/// <summary>
/// Access decision preview service.
/// Previews what would happen if a given identity attempted an action.
/// Delegates evaluation to the runtime adapter — does NOT compute locally.
///
/// Advisory only — does NOT enforce or grant access.
///
/// MUST NOT:
/// - Evaluate policy rules locally
/// - Assign or modify permissions
/// - Embed access logic
/// </summary>
public interface IAccessPreviewService
{
    Task<AccessDecisionPreviewView> PreviewAsync(
        Guid identityId,
        string resource,
        string action,
        string correlationId,
        CancellationToken cancellationToken = default);
}
