using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
// ReSharper disable NotResolvedInText

namespace EmailService;

public class EmailService : IEmailService
{
    private readonly string _smtpHost;
    private readonly string _smtpPassword;
    private readonly int _smtpPort;
    private readonly string _smtpUser;

    public EmailService(IConfiguration configuration)
    {
        _smtpHost = configuration["Smtp:Host"] ?? throw new ArgumentNullException("Smtp:Host is not configured");
        _smtpPort = int.Parse(configuration["Smtp:Port"] ??
                              throw new ArgumentNullException("Smtp:Port is not configured"));
        _smtpUser = configuration["Smtp:User"] ?? throw new ArgumentNullException("Smtp:User is not configured");
        _smtpPassword = configuration["Smtp:Password"] ??
                        throw new ArgumentNullException("Smtp:Password is not configured");
    }

    public async Task SendConfirmationEmailAsync(string email, string code, CancellationToken cancellationToken)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("HighTime", _smtpUser));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = "Email Confirmation";
        message.Body = new TextPart("html")
        {
            Text = $@"
        <html>
        <head>
            <style>
                body {{
                    font-family: Arial, sans-serif;
                    background-color: #f4f4f4;
                    text-align: center;
                    padding: 20px;
                }}
                .container {{
                    background-color: #ffffff;
                    padding: 20px;
                    border-radius: 8px;
                    box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                    max-width: 400px;
                    margin: auto;
                }}
                .code {{
                    font-size: 24px;
                    font-weight: bold;
                    color: #007bff;
                    margin: 20px 0;
                }}
                .footer {{
                    font-size: 12px;
                    color: #666;
                    margin-top: 20px;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <h2>Confirm Your Registration</h2>
                <h3>Project in Development, sorry if service spamming or kill your family</h3>
                <p>Your confirmation code is:</p>
                <div class='code'>{code}</div>
                <p>Please enter this code to complete your registration.</p>
                <div class='footer'>If you did not request this, please ignore this email.</div>
            </div>
        </body>
        </html>"
        };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.Auto, cancellationToken);
            await client.AuthenticateAsync(_smtpUser, _smtpPassword, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException("Email sending operation timed out after 10 seconds.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
        }
    }
}