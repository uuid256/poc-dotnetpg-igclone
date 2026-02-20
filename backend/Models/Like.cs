using System.Diagnostics.CodeAnalysis;

namespace InstaClone.Api.Models;

[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Domain model name; not consumed by VB.NET")]
public class Like
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;
}
