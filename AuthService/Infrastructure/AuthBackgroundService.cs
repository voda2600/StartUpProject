using System.Threading.Channels;
using EmailService;

namespace AuthService.Infrastructure;

public class AuthBackgroundService : BackgroundService
{
    private const int MaxRetryCount = 3;
    private const int MaxParallelTasks = 10;
    private const int BaseDelaySeconds = 30;
    private readonly Channel<(string Email, string Code, int RetryCount)> _emailQueue;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthBackgroundService> _logger;

    public AuthBackgroundService(ILogger<AuthBackgroundService> logger, IEmailService emailService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _emailQueue = Channel.CreateBounded<(string, string, int)>(100);
        _logger.LogInformation("AuthBackgroundService initialized with bounded channel of size 100");
    }

    public async Task QueueEmailAsync(string email, string code)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
        {
            _logger.LogWarning("Invalid email or code provided for queuing");
            return;
        }

        _logger.LogInformation("Attempting to queue email for {Email} with code {Code}", email, code);
        await _emailQueue.Writer.WriteAsync((email, code, 0), CancellationToken.None);
        _logger.LogInformation("Queued email for {Email} with code {Code}", email, code);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return ProcessQueueAsync(stoppingToken);
    }

    private async Task ProcessQueueAsync(CancellationToken stoppingToken)
    {
        var semaphore = new SemaphoreSlim(MaxParallelTasks);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!await _emailQueue.Reader.WaitToReadAsync(stoppingToken))
                {
                    _logger.LogWarning(
                        "Failed to wait for email queue to read, stopping processing. Token state: {IsCanceled}",
                        stoppingToken.IsCancellationRequested);
                    break;
                }

                var tasks = new List<Task>();

                while (_emailQueue.Reader.TryRead(out var emailData))
                {
                    _logger.LogInformation("Processing email for {Email}, retry count: {RetryCount}", emailData.Email,
                        emailData.RetryCount);
                    await semaphore.WaitAsync(stoppingToken);
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await ProcessEmailAsync(emailData, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Unexpected error in task processing email for {Email}",
                                emailData.Email);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }, stoppingToken));
                }

                _logger.LogInformation("Waiting for {TaskCount} tasks to complete...", tasks.Count);
                await Task.WhenAll(tasks);
                _logger.LogInformation("All tasks completed successfully");
            }
        }
        catch (OperationCanceledException)
        {
            /*ignore*/
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error in ProcessQueueAsync, stopping processing");
        }
        finally
        {
            _logger.LogInformation("ProcessQueueAsync stopping...");
        }
    }

    private async Task ProcessEmailAsync((string Email, string Code, int RetryCount) emailData,
        CancellationToken stoppingToken)
    {
        var (email, code, retryCount) = emailData;

        _logger.LogInformation("Starting to process email for {Email}, retry count: {RetryCount}", email, retryCount);

        if (retryCount > MaxRetryCount)
        {
            _logger.LogError("Max retries reached for {Email}, skipping", email);
            return;
        }

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, stoppingToken).Token;

            _logger.LogInformation("Attempting to send email to {Email}", email);
            await _emailService.SendConfirmationEmail(email, code, linkedToken);
            _logger.LogInformation("Successfully sent email to {Email}", email);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Email sending timed out for {Email} after 15 seconds", email);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            AddToQueueWithDelay(email, code, retryCount + 1, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex.Message, "An error occurred while sending email to {Email}", email);
            AddToQueueWithDelay(email, code, retryCount + 1, stoppingToken);
        }
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }

    private async Task AddToQueueWithDelay(string email, string code, int retryCount, CancellationToken stoppingToken)
    {
        var delaySeconds = BaseDelaySeconds * retryCount;
        _logger.LogInformation("Scheduling retry for {Email} in {DelaySeconds} seconds, retry count: {RetryCount}",
            email, delaySeconds, retryCount);

        await Task.Delay(delaySeconds * 1000, stoppingToken);

        if (!stoppingToken.IsCancellationRequested)
        {
            await _emailQueue.Writer.WriteAsync((email, code, retryCount), stoppingToken);
            _logger.LogInformation("Retried email queued for {Email} with code {Code}, retry count: {RetryCount}",
                email, code, retryCount);
        }
        else
        {
            _logger.LogWarning("Retry for {Email} canceled due to stopping token", email);
        }
    }
}