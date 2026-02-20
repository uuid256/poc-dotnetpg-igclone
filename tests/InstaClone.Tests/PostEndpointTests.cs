using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using InstaClone.Api.Dtos;
using InstaClone.Tests.Helpers;

namespace InstaClone.Tests;

public class PostEndpointTests : IClassFixture<InstaCloneAppFactory>
{
    private readonly HttpClient _client;

    public PostEndpointTests(InstaCloneAppFactory factory)
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

    // --- Create ---

    [Fact]
    public async Task Create_Unauthenticated_Returns401()
    {
        using var content = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(imageContent, "image", "test.jpg");

        var response = await _client.PostAsync("/api/posts", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_ValidRequest_Returns201()
    {
        var token = await LoginAsSeedUserAsync();

        using var content = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(imageContent, "image", "test.jpg");
        content.Add(new StringContent("Integration test caption"), "caption");

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/posts");
        request.Content = content;
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var post = await response.Content.ReadFromJsonAsync<PostResponse>();
        Assert.NotNull(post);
        Assert.Equal("Integration test caption", post.Caption);
        Assert.Equal(0, post.CommentCount);
        Assert.Equal(0, post.LikeCount);
    }

    // --- Get ---

    [Fact]
    public async Task Get_ExistingPost_Returns200()
    {
        var feed = await _client.GetFromJsonAsync<FeedResponse>("/api/posts/feed");
        var postId = feed!.Posts.First().Id;

        var response = await _client.GetAsync($"/api/posts/{postId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var post = await response.Content.ReadFromJsonAsync<PostResponse>();
        Assert.Equal(postId, post!.Id);
    }

    [Fact]
    public async Task Get_NonexistentPost_Returns404()
    {
        var response = await _client.GetAsync($"/api/posts/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // --- Feed ---

    [Fact]
    public async Task Feed_DefaultPageSize_ReturnsPosts()
    {
        var response = await _client.GetFromJsonAsync<FeedResponse>("/api/posts/feed");

        Assert.NotNull(response);
        Assert.NotEmpty(response.Posts);
        Assert.Equal(1, response.Page);
        Assert.Equal(10, response.PageSize);
    }

    [Fact]
    public async Task Feed_CustomPageSize_RespectsLimit()
    {
        var response = await _client.GetFromJsonAsync<FeedResponse>("/api/posts/feed?pageSize=2");

        Assert.Equal(2, response!.Posts.Count);
        Assert.Equal(2, response.PageSize);
    }

    [Fact]
    public async Task Feed_PageSizeClampedToMinAndMax()
    {
        var minResponse = await _client.GetFromJsonAsync<FeedResponse>("/api/posts/feed?pageSize=0");
        Assert.Equal(1, minResponse!.PageSize);
        Assert.Single(minResponse.Posts);

        var maxResponse = await _client.GetFromJsonAsync<FeedResponse>("/api/posts/feed?pageSize=100");
        Assert.Equal(50, maxResponse!.PageSize);
    }

    [Fact]
    public async Task Feed_HasMoreFlag_CorrectWhenMorePosts()
    {
        var smallPage = await _client.GetFromJsonAsync<FeedResponse>("/api/posts/feed?pageSize=2");
        Assert.True(smallPage!.HasMore);

        var largePage = await _client.GetFromJsonAsync<FeedResponse>("/api/posts/feed?pageSize=50");
        Assert.False(largePage!.HasMore);
    }

    [Fact]
    public async Task Feed_NewestFirstOrder()
    {
        var response = await _client.GetFromJsonAsync<FeedResponse>("/api/posts/feed");
        var posts = response!.Posts;

        for (int i = 1; i < posts.Count; i++)
        {
            Assert.True(posts[i - 1].CreatedAt >= posts[i].CreatedAt,
                "Posts should be ordered newest first");
        }
    }
}
