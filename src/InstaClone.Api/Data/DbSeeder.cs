using InstaClone.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InstaClone.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync())
            return;

        // --- Users ---
        var alice = new User
        {
            Username = "alice",
            Email = "alice@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
            DisplayName = "Alice Johnson",
            Bio = "Photographer and coffee lover"
        };
        var bob = new User
        {
            Username = "bob",
            Email = "bob@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
            DisplayName = "Bob Smith",
            Bio = "Travel enthusiast"
        };
        var charlie = new User
        {
            Username = "charlie",
            Email = "charlie@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
            DisplayName = "Charlie Davis",
            Bio = "Foodie and amateur chef"
        };

        db.Users.AddRange(alice, bob, charlie);
        await db.SaveChangesAsync();

        // --- Generate placeholder images ---
        var uploadsPath = "/app/uploads";
        Directory.CreateDirectory(uploadsPath);

        var imageFiles = new List<string>();
        for (var i = 1; i <= 6; i++)
        {
            var fileName = $"seed-{i}.svg";
            var filePath = Path.Combine(uploadsPath, fileName);
            if (!File.Exists(filePath))
            {
                var hue = i * 60;
                var svg = $"""
                    <svg xmlns="http://www.w3.org/2000/svg" width="800" height="800">
                      <rect width="800" height="800" fill="hsl({hue}, 60%, 80%)"/>
                      <text x="400" y="420" text-anchor="middle" font-family="sans-serif" font-size="64" fill="hsl({hue}, 40%, 30%)">Photo {i}</text>
                    </svg>
                    """;
                await File.WriteAllTextAsync(filePath, svg);
            }
            imageFiles.Add(fileName);
        }

        // --- Posts ---
        var posts = new[]
        {
            new Post { ImageUrl = $"/uploads/{imageFiles[0]}", Caption = "Morning coffee vibes", UserId = alice.Id, CreatedAt = DateTime.UtcNow.AddHours(-12) },
            new Post { ImageUrl = $"/uploads/{imageFiles[1]}", Caption = "Sunset at the beach", UserId = bob.Id, CreatedAt = DateTime.UtcNow.AddHours(-10) },
            new Post { ImageUrl = $"/uploads/{imageFiles[2]}", Caption = "Homemade pasta night", UserId = charlie.Id, CreatedAt = DateTime.UtcNow.AddHours(-8) },
            new Post { ImageUrl = $"/uploads/{imageFiles[3]}", Caption = "City lights from above", UserId = alice.Id, CreatedAt = DateTime.UtcNow.AddHours(-6) },
            new Post { ImageUrl = $"/uploads/{imageFiles[4]}", Caption = "Weekend hiking adventure", UserId = bob.Id, CreatedAt = DateTime.UtcNow.AddHours(-4) },
            new Post { ImageUrl = $"/uploads/{imageFiles[5]}", Caption = "Fresh baked sourdough", UserId = charlie.Id, CreatedAt = DateTime.UtcNow.AddHours(-2) },
        };

        db.Posts.AddRange(posts);
        await db.SaveChangesAsync();

        // --- Comments ---
        var comments = new[]
        {
            new Comment { Text = "Love this shot", UserId = bob.Id, PostId = posts[0].Id },
            new Comment { Text = "So beautiful", UserId = charlie.Id, PostId = posts[0].Id },
            new Comment { Text = "Where is this?", UserId = alice.Id, PostId = posts[1].Id },
            new Comment { Text = "Looks delicious", UserId = alice.Id, PostId = posts[2].Id },
            new Comment { Text = "Recipe please", UserId = bob.Id, PostId = posts[2].Id },
            new Comment { Text = "Amazing view", UserId = charlie.Id, PostId = posts[3].Id },
            new Comment { Text = "I need to go there", UserId = charlie.Id, PostId = posts[4].Id },
            new Comment { Text = "Teach me your ways", UserId = alice.Id, PostId = posts[5].Id },
        };

        db.Comments.AddRange(comments);
        await db.SaveChangesAsync();

        // --- Likes ---
        var likes = new[]
        {
            new Like { UserId = bob.Id, PostId = posts[0].Id },
            new Like { UserId = charlie.Id, PostId = posts[0].Id },
            new Like { UserId = alice.Id, PostId = posts[1].Id },
            new Like { UserId = charlie.Id, PostId = posts[1].Id },
            new Like { UserId = alice.Id, PostId = posts[2].Id },
            new Like { UserId = bob.Id, PostId = posts[2].Id },
            new Like { UserId = bob.Id, PostId = posts[3].Id },
            new Like { UserId = charlie.Id, PostId = posts[3].Id },
            new Like { UserId = alice.Id, PostId = posts[4].Id },
            new Like { UserId = alice.Id, PostId = posts[5].Id },
            new Like { UserId = bob.Id, PostId = posts[5].Id },
        };

        db.Likes.AddRange(likes);
        await db.SaveChangesAsync();
    }
}
