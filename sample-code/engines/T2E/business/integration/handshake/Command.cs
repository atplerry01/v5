namespace Whycespace.Engines.T2E.Business.Integration.Handshake;

public record HandshakeCommand(
    string Action,
    string EntityId,
    object Payload
);
