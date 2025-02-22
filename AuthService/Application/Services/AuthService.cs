using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Enities;
using AuthService.Domain.Interfaces;
using AuthService.Infrastructure;

namespace AuthService.Application.Services;

public class AuthService : IAuthService
{
    private readonly AuthBackgroundService _backgroundService;
    private readonly TimeSpan _codeRetryInterval = TimeSpan.FromMinutes(2);
    private readonly TimeSpan _expiredSpan = TimeSpan.FromMinutes(30);
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository, IJwtTokenGenerator tokenGenerator,
        AuthBackgroundService backgroundService)
    {
        _userRepository = userRepository;
        _tokenGenerator = tokenGenerator;
        _backgroundService = backgroundService;
    }

    public async Task<string> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new BadHttpRequestException("User already exists");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var code = GenerateConfirmationCode();
        var user = new User(request.Email, passwordHash, request.Name, code, GetExpiredDateTime());

        await _backgroundService.QueueEmailAsync(user.Email, user.ConfirmationCode!);
        await _userRepository.AddAsync(user);
        return _tokenGenerator.GenerateToken(user);
    }


    public async Task<(string?, bool)> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || User.VerifyPassword(request.Password, user.PasswordHash))
            throw new BadHttpRequestException("Invalid credentials");

        return (_tokenGenerator.GenerateToken(user), user.EmailConfirmed);
    }

    public async Task<bool> ConfirmEmailCode(ConfirmEmailRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.EmailFromToken);
        if (user == null)
            throw new BadHttpRequestException("Invalid email");

        if (user.ConfirmationCodeExpiration < DateTime.UtcNow || string.IsNullOrEmpty(request.ConfirmationCode) ||
            user.ConfirmationCode != request.ConfirmationCode) return false;
        user.ConfirmationCode = null;
        user.ConfirmationCodeExpiration = null;
        user.EmailConfirmed = true;
        await _userRepository.UpdateAsync(user);
        return true;
    }

    public async Task UpdateEmailCode(UpdateCodeRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.EmailFromToken);
        if (user == null)
            throw new BadHttpRequestException("Invalid email");

        if ((user.ConfirmationCodeExpiration.HasValue && CanRetry(user.ConfirmationCodeExpiration.Value)) ||
            !user.ConfirmationCodeExpiration.HasValue)
        {
            user.ConfirmationCode = GenerateConfirmationCode();
            user.ConfirmationCodeExpiration = GetExpiredDateTime();
            user.EmailConfirmed = false;
            await _userRepository.UpdateAsync(user);
            await _backgroundService.QueueEmailAsync(user.Email, user.ConfirmationCode!);
        }
        else
        {
            throw new BadHttpRequestException("Wait for retry delay");
        }
    }

    private DateTime GetExpiredDateTime()
    {
        return DateTime.UtcNow.Add(_expiredSpan);
    }

    private bool CanRetry(DateTime time)
    {
        return time.Add(-_expiredSpan).Add(_codeRetryInterval) <= DateTime.UtcNow;
    }

    private static string GenerateConfirmationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}