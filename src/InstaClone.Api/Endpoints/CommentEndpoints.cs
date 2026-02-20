using System.Security.Claims;
using InstaClone.Api.Data;
using InstaClone.Api.Dtos;
using InstaClone.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InstaClone.Api.Endpoints;

public static class CommentEndpoints
{
    public static void MapCommentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/posts/{postId:guid}/comments").WithTags("Comments");

        group.MapPost("/", async (
            Guid postId,
            CreateCommentRequest request,
            ClaimsPrincipal claims,
            AppDbContext db) =>
        {
            var userId = Guid.Parse(claims.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (!await db.Posts.AnyAsync(p => p.Id == postId))
                return Results.NotFound(new { error = "Post not found." });

            var comment = new Comment
            {
                Text = request.Text,
                UserId = userId,
                PostId = postId
            };

            db.Comments.Add(comment);
            await db.SaveChangesAsync();

            var user = await db.Users.FindAsync(userId);
            return Results.Created($"/api/posts/{postId}/comments",
                new CommentResponse(comment.Id, comment.Text, comment.CreatedAt, userId, user!.Username));
        })
        .RequireAuthorization();

        group.MapGet("/", async (Guid postId, AppDbContext db) =>
        {
            var comments = await db.Comments
                .Where(c => c.PostId == postId)
                .OrderByDescending(c => c.CreatedAt)
                .Include(c => c.User)
                .Select(c => new CommentResponse(
                    c.Id, c.Text, c.CreatedAt, c.UserId, c.User.Username))
                .ToListAsync();

            return Results.Ok(comments);
        });
    }
}
