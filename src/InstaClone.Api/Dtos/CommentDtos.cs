namespace InstaClone.Api.Dtos;

public record CreateCommentRequest(string Text);

public record CommentResponse(
    Guid Id,
    string Text,
    DateTime CreatedAt,
    Guid UserId,
    string Username
);
