namespace InstaClone.Api.Dtos;

public record RegisterRequest(string Username, string Email, string Password, string? DisplayName);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, Guid UserId, string Username);
