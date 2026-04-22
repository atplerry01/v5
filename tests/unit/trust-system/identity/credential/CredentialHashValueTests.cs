using Whycespace.Domain.TrustSystem.Identity.Credential;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.TrustSystem.Identity.Credential;

/// <summary>
/// 2.8.21 — Certification tests for CredentialHashValue.
/// Verifies domain-level enforcement that prevents plaintext credential storage.
/// </summary>
public sealed class CredentialHashValueTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static CredentialId NewId(string seed) =>
        new(IdGen.Generate($"CredentialHashValueTests:{seed}"));

    [Fact]
    public void CredentialHashValue_WithValidHash_IsCreated()
    {
        var hash = new CredentialHashValue("$2a$12$abcdefghijklmnopqrstuuABCDEFGHIJKLMNOPQRSTUVWXYZ01234");

        Assert.Equal("$2a$12$abcdefghijklmnopqrstuuABCDEFGHIJKLMNOPQRSTUVWXYZ01234", hash.Value);
    }

    [Fact]
    public void CredentialHashValue_WithEmptyString_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new CredentialHashValue(""));
    }

    [Fact]
    public void CredentialHashValue_WithWhitespaceOnly_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new CredentialHashValue("   "));
    }

    [Fact]
    public void CredentialHashValue_ShortThanMinimum_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new CredentialHashValue("tooshort"));
    }

    [Fact]
    public void CredentialHashValue_ExactlyAtMinimumLength_IsCreated()
    {
        var minHash = new string('x', 20);

        var hash = new CredentialHashValue(minHash);

        Assert.Equal(minHash, hash.Value);
    }

    [Fact]
    public void CredentialHashValue_ImplicitConversion_ReturnsValue()
    {
        var hashString = "$2a$12$longenoughhashabcdefghijklmnopqrstuvwxyz01234567890";
        var hash = new CredentialHashValue(hashString);

        string converted = hash;

        Assert.Equal(hashString, converted);
    }

    [Fact]
    public void CredentialDescriptor_WithHash_PreservesHashOnRoundtrip()
    {
        var identityRef = IdGen.Generate("CredentialHashValueTests:descriptor:identity");
        var hashString = "$2a$12$longenoughhashabcdefghijklmnopqrstuvwxyz01234567890";
        var descriptor = new CredentialDescriptor(
            identityRef,
            "Password",
            new CredentialHashValue(hashString));

        Assert.NotNull(descriptor.CredentialHash);
        Assert.Equal(hashString, descriptor.CredentialHash!.Value.Value);
    }

    [Fact]
    public void CredentialDescriptor_WithoutHash_HasNullHash()
    {
        var identityRef = IdGen.Generate("CredentialHashValueTests:descriptor:nohash");
        var descriptor = new CredentialDescriptor(identityRef, "ApiKey");

        Assert.Null(descriptor.CredentialHash);
    }

    [Fact]
    public void CredentialAggregate_Issue_WithHash_PreservesHashInDescriptor()
    {
        var id = NewId("Issue_WithHash");
        var identityRef = IdGen.Generate("CredentialHashValueTests:issue:identity");
        var hashString = "$2a$12$longenoughhashabcdefghijklmnopqrstuvwxyz01234567890";
        var descriptor = new CredentialDescriptor(
            identityRef, "Password", new CredentialHashValue(hashString));

        var aggregate = CredentialAggregate.Issue(id, descriptor);

        Assert.Equal(hashString, aggregate.Descriptor.CredentialHash!.Value.Value);
    }
}
