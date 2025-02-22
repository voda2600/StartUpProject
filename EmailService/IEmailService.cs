namespace EmailService;

public interface IEmailService
{
    Task SendConfirmationEmail(string email, string code, CancellationToken cancellationToken);
}