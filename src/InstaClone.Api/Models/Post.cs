namespace InstaClone.Api.Models;

public class Post
{
    public Guid Id { get; set; }
    public required string ImageUrl { get; set; }
    public string? Caption { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public List<Comment> Comments { get; set; } = [];
    public List<Like> Likes { get; set; } = [];
}
