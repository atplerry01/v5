namespace Whycespace.Domain.BusinessSystem.Integration.Token;

public sealed record TokenIssuedEvent(TokenId TokenId, TokenDescriptor Descriptor);
public sealed record TokenActivatedEvent(TokenId TokenId);
public sealed record TokenExpiredEvent(TokenId TokenId);
public sealed record TokenRevokedEvent(TokenId TokenId);
