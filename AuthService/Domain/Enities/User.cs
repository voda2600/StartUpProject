using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Domain.Enities;

[Index(nameof(Email), IsUnique = true)]
public class User
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private User()
    {
    }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public User(string email, string passwordHash, string name)
    {
        Email = email ?? throw new ArgumentNullException(nameof(email));
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public Guid Id { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(120)]
    public string Email { get; set; }

    [Required] public string PasswordHash { get; set; }

    [Required] [MaxLength(100)] public string Name { get; set; }

    public bool EmailConfirmed { get; set; } = false;
    public string? ConfirmationCode { get; set; }
    public DateTime? ConfirmationCodeExpiration { get; set; }
    
    [MaxLength(500)]
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    public static bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}