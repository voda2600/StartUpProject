using System.Security.Authentication;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Enities;
using AuthService.Domain.Interfaces;

namespace AuthService.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public AuthService(IUserRepository userRepository, IJwtTokenGenerator tokenGenerator)
    {
        _userRepository = userRepository;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<string> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new AuthenticationException("User already exists");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new User(request.Email, passwordHash, request.Name);

        await _userRepository.AddAsync(user);
        return _tokenGenerator.GenerateToken(user);
    }

    public async Task<string> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || !user.VerifyPassword(request.Password, user.PasswordHash))
            throw new AuthenticationException("Invalid credentials");

        return _tokenGenerator.GenerateToken(user);
    }
}