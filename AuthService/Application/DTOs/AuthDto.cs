namespace AuthService.Application.DTOs;

public record RegisterRequest(string Email, string Password, string Name);
public record LoginRequest(string Email, string Password);
public record UserDto(Guid Id, string Email, string Name);