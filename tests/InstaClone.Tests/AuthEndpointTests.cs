using System.Net;
using System.Net.Http.Json;
using InstaClone.Api.Dtos;
using InstaClone.Tests.Helpers;

namespace InstaClone.Tests;

public class AuthEndpointTests : IClassFixture<InstaCloneAppFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointTests(InstaCloneAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    // --- Register validation ---

    [Fact]
    public async Task Register_MissingUsername_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "test@example.com",
            Password = "password123"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_UsernameTooShort_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Username = "ab",
            Email = "test@example.com",
            Password = "password123"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_UsernameTooLong_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Username = new string('a', 31),
            Email = "test@example.com",
            Password = "password123"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_UsernameInvalidChars_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Username = "bad-user!",
            Email = "test@example.com",
            Password = "password123"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_MissingEmail_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Username = "validuser",
            Password = "password123"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_InvalidEmail_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Username = "validuser",
            Email = "notanemail",
            Password = "password123"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_MissingPassword_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Username = "validuser",
            Email = "valid@example.com"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_PasswordTooShort_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Username = "validuser",
            Email = "valid@example.com",
            Password = "12345"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // --- Register success + duplicates ---

    [Fact]
    public async Task Register_ValidRequest_ReturnsOkWithToken()
    {
        var unique = Guid.NewGuid().ToString("N")[..12];
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Username = $"user_{unique}",
            Email = $"{unique}@example.com",
            Password = "password123"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrWhiteSpace(auth.Token));
        Assert.NotEqual(Guid.Empty, auth.UserId);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Username = "uniqueuser99",
            Email = "alice@example.com", // seed data
            Password = "password123"
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateUsername_Returns409()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Username = "alice", // seed data
            Email = "unique99@example.com",
            Password = "password123"
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // --- Login ---

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "alice@example.com",
            Password = "password"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrWhiteSpace(auth.Token));
        Assert.Equal("alice", auth.Username);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "alice@example.com",
            Password = "wrongpassword"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_NonexistentEmail_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "nobody@example.com",
            Password = "password"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
