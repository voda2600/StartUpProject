using System.ComponentModel.DataAnnotations;

namespace AuthService.Domain.Enities;

public class User
{
    public Guid Id { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(120)]
    public string Email { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private User()
    { }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public User(string email, string passwordHash, string name)
    {
        Email = email ?? throw new ArgumentNullException(nameof(email));
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}