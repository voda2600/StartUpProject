using AuthService.Domain.Enities;

namespace AuthService.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);

    Task<User?> GetByIdAsync(Guid id);

    Task AddAsync(User user);

    Task UpdateAsync(User user);

    Task DeleteAsync(Guid id);
    
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
}