using AuthService.Domain.Enities;

namespace AuthService.Application.Interfaces;

public interface IJwtTokenService
{
    Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(User user);
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    bool ValidateRefreshToken(string refreshToken, out User? user);
}