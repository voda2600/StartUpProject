namespace AuthService.Application.DTOs;

public record RegisterRequest(string Email, string Password, string Name);

public record LoginRequest(string Email, string Password);

public record UserDto(Guid Id, string Email, string Name);

public record ConfirmEmailRequest(string EmailFromToken, string ConfirmationCode);

public record UpdateCodeRequest(string EmailFromToken);

public record RefreshTokenRequest(string RefreshToken);