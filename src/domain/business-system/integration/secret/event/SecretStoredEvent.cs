namespace Whycespace.Domain.BusinessSystem.Integration.Secret;

public sealed record SecretStoredEvent(SecretId SecretId, SecretDescriptor Descriptor);
