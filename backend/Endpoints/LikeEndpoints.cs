using System.Security.Claims;
using InstaClone.Api.Data;
using InstaClone.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InstaClone.Api.Endpoints;

public static class LikeEndpoints
{
    public static void MapLikeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/posts/{postId:guid}/likes").WithTags("Likes");

        group.MapPost("/", async (Guid postId, ClaimsPrincipal claims, AppDbContext db) =>
        {
            var userId = Guid.Parse(claims.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (!await db.Posts.AnyAsync(p => p.Id == postId))
                return Results.NotFound(new { error = "Post not found." });

            if (await db.Likes.AnyAsync(l => l.UserId == userId && l.PostId == postId))
                return Results.Conflict(new { error = "Already liked." });

            var like = new Like
            {
                UserId = userId,
                PostId = postId
            };

            db.Likes.Add(like);
            await db.SaveChangesAsync();

            return Results.Ok(new { message = "Post liked." });
        })
        .RequireAuthorization();

        group.MapDelete("/", async (Guid postId, ClaimsPrincipal claims, AppDbContext db) =>
        {
            var userId = Guid.Parse(claims.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var like = await db.Likes
                .FirstOrDefaultAsync(l => l.UserId == userId && l.PostId == postId);

            if (like is null)
                return Results.NotFound(new { error = "Like not found." });

            db.Likes.Remove(like);
            await db.SaveChangesAsync();

            return Results.Ok(new { message = "Post unliked." });
        })
        .RequireAuthorization();
    }
}
