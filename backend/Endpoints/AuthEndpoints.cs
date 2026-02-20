using System.Text.RegularExpressions;
using InstaClone.Api.Data;
using InstaClone.Api.Dtos;
using InstaClone.Api.Models;
using InstaClone.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace InstaClone.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterRequest request, AppDbContext db, TokenService tokenService) =>
        {
            // Username validation
            if (string.IsNullOrWhiteSpace(request.Username))
                return Results.BadRequest(new { error = "Username is required." });
            if (request.Username.Length < 3 || request.Username.Length > 30)
                return Results.BadRequest(new { error = "Username must be between 3 and 30 characters." });
            if (!Regex.IsMatch(request.Username, @"^[a-zA-Z0-9_]+$"))
                return Results.BadRequest(new { error = "Username can only contain letters, numbers, and underscores." });

            // Email validation
            if (string.IsNullOrWhiteSpace(request.Email))
                return Results.BadRequest(new { error = "Email is required." });
            var atIndex = request.Email.IndexOf('@');
            if (atIndex < 1 || request.Email.IndexOf('.', atIndex) < 0)
                return Results.BadRequest(new { error = "Email is not valid." });

            // Password validation
            if (string.IsNullOrEmpty(request.Password))
                return Results.BadRequest(new { error = "Password is required." });
            if (request.Password.Length < 6)
                return Results.BadRequest(new { error = "Password must be at least 6 characters." });

            if (await db.Users.AnyAsync(u => u.Email == request.Email))
                return Results.Conflict(new { error = "Email already registered." });

            if (await db.Users.AnyAsync(u => u.Username == request.Username))
                return Results.Conflict(new { error = "Username already taken." });

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                DisplayName = request.DisplayName
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var token = tokenService.GenerateToken(user.Id, user.Username);
            return Results.Ok(new AuthResponse(token, user.Id, user.Username));
        });

        group.MapPost("/login", async (LoginRequest request, AppDbContext db, TokenService tokenService) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Results.Unauthorized();

            var token = tokenService.GenerateToken(user.Id, user.Username);
            return Results.Ok(new AuthResponse(token, user.Id, user.Username));
        });
    }
}
