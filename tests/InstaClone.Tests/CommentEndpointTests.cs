using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using InstaClone.Api.Dtos;
using InstaClone.Tests.Helpers;

namespace InstaClone.Tests;

public class CommentEndpointTests : IClassFixture<InstaCloneAppFactory>
{
    private readonly HttpClient _client;

    public CommentEndpointTests(InstaCloneAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> LoginAsSeedUserAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "alice@example.com",
            Password = "password"
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
    public async Task Add_ValidComment_Returns201()
    {
        var token = await LoginAsSeedUserAsync();
        var postId = await GetFirstPostIdAsync();

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/posts/{postId}/comments");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new { Text = "Great post!" });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var comment = await response.Content.ReadFromJsonAsync<CommentResponse>();
        Assert.NotNull(comment);
        Assert.Equal("Great post!", comment.Text);
    }

    [Fact]
    public async Task Add_NonexistentPost_Returns404()
    {
        var token = await LoginAsSeedUserAsync();

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/posts/{Guid.NewGuid()}/comments");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new { Text = "Comment" });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Add_Unauthenticated_Returns401()
    {
        var postId = await GetFirstPostIdAsync();

        var response = await _client.PostAsJsonAsync($"/api/posts/{postId}/comments",
            new { Text = "test" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task List_ReturnsComments_NewestFirst()
    {
        var token = await LoginAsSeedUserAsync();
        var postId = await GetFirstPostIdAsync();

        // Add a comment so we know at least one exists on this post
        using var addReq = new HttpRequestMessage(HttpMethod.Post, $"/api/posts/{postId}/comments");
        addReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        addReq.Content = JsonContent.Create(new { Text = "Ordering test comment" });
        await _client.SendAsync(addReq);

        var comments = await _client.GetFromJsonAsync<List<CommentResponse>>(
            $"/api/posts/{postId}/comments");

        Assert.NotNull(comments);
        Assert.NotEmpty(comments);

        for (int i = 1; i < comments.Count; i++)
        {
            Assert.True(comments[i - 1].CreatedAt >= comments[i].CreatedAt,
                "Comments should be ordered newest first");
        }
    }
}
