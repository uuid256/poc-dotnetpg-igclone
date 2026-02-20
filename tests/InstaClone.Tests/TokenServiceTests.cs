using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using InstaClone.Api.Services;
using Microsoft.Extensions.Configuration;

namespace InstaClone.Tests;

public class TokenServiceTests
{
    private readonly TokenService _tokenService;
    private readonly Guid _userId = Guid.NewGuid();
    private const string Username = "testuser";

    public TokenServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "super-secret-key-for-dev-only-change-in-prod-minimum-32-chars!!",
                ["Jwt:Issuer"] = "InstaClone",
                ["Jwt:Audience"] = "InstaClone",
                ["Jwt:ExpiryInMinutes"] = "60"
            })
            .Build();

        _tokenService = new TokenService(config);
    }

    [Fact]
    public void GenerateToken_ReturnsValidJwt()
    {
        var token = _tokenService.GenerateToken(_userId, Username);

        Assert.False(string.IsNullOrWhiteSpace(token));
        var handler = new JwtSecurityTokenHandler();
        Assert.True(handler.CanReadToken(token));
    }

    [Fact]
    public void GenerateToken_ContainsCorrectClaims()
    {
        var token = _tokenService.GenerateToken(_userId, Username);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.True(
            jwt.Claims.Any(c =>
                (c.Type == "nameid" || c.Type == ClaimTypes.NameIdentifier)
                && c.Value == _userId.ToString()),
            "Token should contain user ID claim");

        Assert.True(
            jwt.Claims.Any(c =>
                (c.Type == "unique_name" || c.Type == ClaimTypes.Name)
                && c.Value == Username),
            "Token should contain username claim");
    }

    [Fact]
    public void GenerateToken_HasCorrectIssuerAndAudience()
    {
        var token = _tokenService.GenerateToken(_userId, Username);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("InstaClone", jwt.Issuer);
        Assert.Contains("InstaClone", jwt.Audiences);
    }

    [Fact]
    public void GenerateToken_ExpiresInFuture()
    {
        var token = _tokenService.GenerateToken(_userId, Username);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.True(jwt.ValidTo > DateTime.UtcNow);
    }
}
