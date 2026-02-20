using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using InstaClone.Api.Dtos;
using InstaClone.Tests.Helpers;

namespace InstaClone.Tests;

public class LikeEndpointTests : IClassFixture<InstaCloneAppFactory>
{
    private readonly HttpClient _client;

    public LikeEndpointTests(InstaCloneAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterNewUserAsync()
    {
        var unique = Guid.NewGuid().ToString("N")[..12];
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Username = $"liker_{unique}",
            Email = $"liker_{unique}@test.com",
            Password = "password123"
        });
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return auth!.Token;
    }

    private async Task<Guid> GetFirstPostIdAsync()
    {
        var feed = await _client.GetFromJsonAsync<FeedResponse>("/api/posts/feed");
        return feed!.Posts.First().Id;
    }

    [Fact]
    public async Task Like_ValidPost_Returns200()
    {
        var token = await RegisterNewUserAsync();
        var postId = await GetFirstPostIdAsync();

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/posts/{postId}/likes");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Like_NonexistentPost_Returns404()
    {
        var token = await RegisterNewUserAsync();

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/posts/{Guid.NewGuid()}/likes");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Like_AlreadyLiked_Returns409()
    {
        var token = await RegisterNewUserAsync();
        var postId = await GetFirstPostIdAsync();

        // First like
        using var req1 = new HttpRequestMessage(HttpMethod.Post, $"/api/posts/{postId}/likes");
        req1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await _client.SendAsync(req1);

        // Second like (duplicate)
        using var req2 = new HttpRequestMessage(HttpMethod.Post, $"/api/posts/{postId}/likes");
        req2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(req2);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Like_Unauthenticated_Returns401()
    {
        var postId = await GetFirstPostIdAsync();

        var response = await _client.PostAsync($"/api/posts/{postId}/likes", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Unlike_ValidLike_Returns200()
    {
        var token = await RegisterNewUserAsync();
        var postId = await GetFirstPostIdAsync();

        // Like first
        using var likeReq = new HttpRequestMessage(HttpMethod.Post, $"/api/posts/{postId}/likes");
        likeReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await _client.SendAsync(likeReq);

        // Unlike
        using var unlikeReq = new HttpRequestMessage(HttpMethod.Delete, $"/api/posts/{postId}/likes");
        unlikeReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(unlikeReq);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Unlike_NotLiked_Returns404()
    {
        var token = await RegisterNewUserAsync();
        var postId = await GetFirstPostIdAsync();

        using var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/posts/{postId}/likes");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Unlike_Unauthenticated_Returns401()
    {
        var postId = await GetFirstPostIdAsync();

        var response = await _client.DeleteAsync($"/api/posts/{postId}/likes");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
