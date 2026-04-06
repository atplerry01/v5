namespace Whycespace.Engines.T2E.Trust.Identity.Profile;

public record ProfileCommand(
    string Action,
    string EntityId,
    object Payload
);
