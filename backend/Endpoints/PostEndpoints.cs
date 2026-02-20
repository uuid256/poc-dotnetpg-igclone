using System.Security.Claims;
using InstaClone.Api.Data;
using InstaClone.Api.Dtos;
using InstaClone.Api.Models;
using InstaClone.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstaClone.Api.Endpoints;

public static class PostEndpoints
{
    public static void MapPostEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/posts").WithTags("Posts");

        group.MapPost("/", async (
            IFormFile image,
            [FromForm] string? caption,
            ClaimsPrincipal claims,
            AppDbContext db,
            ImageService imageService,
            HttpRequest request) =>
        {
            var userId = Guid.Parse(claims.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var (fileName, error) = await imageService.SaveImageAsync(image);
            if (error is not null)
                return Results.BadRequest(new { error });

            var post = new Post
            {
                ImageUrl = $"/uploads/{fileName}",
                Caption = caption,
                UserId = userId
            };

            db.Posts.Add(post);
            await db.SaveChangesAsync();

            var user = await db.Users.FindAsync(userId);
            return Results.Created($"/api/posts/{post.Id}", new PostResponse(
                post.Id, post.ImageUrl, post.Caption, post.CreatedAt,
                userId, user!.Username, 0, 0));
        })
        .RequireAuthorization()
        .DisableAntiforgery();

        group.MapGet("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var post = await db.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                .Include(p => p.Likes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post is null)
                return Results.NotFound();

            return Results.Ok(new PostResponse(
                post.Id, post.ImageUrl, post.Caption, post.CreatedAt,
                post.UserId, post.User.Username,
                post.Comments.Count, post.Likes.Count));
        });

        group.MapGet("/feed", async (int? page, int? pageSize, AppDbContext db) =>
        {
            var p = page ?? 1;
            var ps = Math.Clamp(pageSize ?? 10, 1, 50);

            var posts = await db.Posts
                .OrderByDescending(post => post.CreatedAt)
                .Skip((p - 1) * ps)
                .Take(ps + 1) // Take one extra to check if there are more
                .Include(post => post.User)
                .Include(post => post.Comments)
                .Include(post => post.Likes)
                .ToListAsync();

            var hasMore = posts.Count > ps;
            var results = posts.Take(ps).Select(post => new PostResponse(
                post.Id, post.ImageUrl, post.Caption, post.CreatedAt,
                post.UserId, post.User.Username,
                post.Comments.Count, post.Likes.Count
            )).ToList();

            return Results.Ok(new FeedResponse(results, p, ps, hasMore));
        });
    }
}
