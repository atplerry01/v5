namespace Whycespace.Engines.T4A.Portals;

public sealed class PortalAccessEngine
{
    public PortalAccessResult Authorize(PortalAccessCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        return new PortalAccessResult(command.PortalId, true, command.UserId);
    }
}

public sealed record PortalAccessCommand(string PortalId, string UserId, string RequestedScope);

public sealed record PortalAccessResult(string PortalId, bool IsAuthorized, string UserId);
