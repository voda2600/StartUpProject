namespace EmailService;

public interface IEmailService
{
    Task SendConfirmationEmailAsync(string email, string code, CancellationToken cancellationToken);
}