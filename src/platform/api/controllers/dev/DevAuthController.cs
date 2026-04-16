// DEV ONLY — REMOVE BEFORE PRODUCTION
// Gated by Auth:DevMode config flag. Returns 404 when disabled.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Whycespace.Platform.Api.Controllers.Dev;

[AllowAnonymous]
[ApiController]
[Route("api/dev/auth")]
[ApiExplorerSettings(GroupName = "dev.auth")]
public sealed class DevAuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public DevAuthController(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost("token")]
    public IActionResult GenerateToken([FromBody] DevTokenRequest request)
    {
        if (!_config.GetValue<bool>("Auth:DevMode"))
            return NotFound();

        var signingKey = _config.GetValue<string>("Jwt:SigningKey")
            ?? throw new InvalidOperationException("Jwt:SigningKey is required.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var roles = request.Roles ?? new[] { "operator" };

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, request.ActorId ?? "dev-actor"),
            new("scope", "api"),
        };
        foreach (var role in roles)
            claims.Add(new Claim("roles", role));

        var issuer = _config.GetValue<string>("Jwt:Issuer") ?? "whycespace";
        var audience = _config.GetValue<string>("Jwt:Audience") ?? "whycespace-api";

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow.AddMinutes(-1),
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return Ok(new
        {
            access_token = new JwtSecurityTokenHandler().WriteToken(token)
        });
    }
}

public sealed class DevTokenRequest
{
    public string? ActorId { get; set; }
    public string[]? Roles { get; set; }
}
