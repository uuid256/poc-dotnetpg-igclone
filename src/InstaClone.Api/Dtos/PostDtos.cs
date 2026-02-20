namespace InstaClone.Api.Dtos;

public record CreatePostRequest(string? Caption);

public record PostResponse(
    Guid Id,
    string ImageUrl,
    string? Caption,
    DateTime CreatedAt,
    Guid UserId,
    string Username,
    int CommentCount,
    int LikeCount
);

public record FeedResponse(List<PostResponse> Posts, int Page, int PageSize, bool HasMore);
