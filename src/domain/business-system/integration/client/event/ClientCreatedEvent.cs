namespace Whycespace.Domain.BusinessSystem.Integration.Client;

public sealed record ClientCreatedEvent(ClientId ClientId, ExternalClientId ExternalId);
