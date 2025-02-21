using AuthService.Domain.Enities;

namespace AuthService.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}