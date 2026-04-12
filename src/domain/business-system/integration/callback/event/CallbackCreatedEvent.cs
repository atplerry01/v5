namespace Whycespace.Domain.BusinessSystem.Integration.Callback;

public sealed record CallbackCreatedEvent(CallbackId CallbackId, CallbackDefinitionId DefinitionId, string CallbackName);
