namespace HangfireDemo.Services;

public interface IEmailService
{
    void SendEmail(string to, string subject, string body);
    void SendDelayedEmail(string to, string subject, string body);
}

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public void SendEmail(string to, string subject, string body)
    {
        // Simulate email sending
        _logger.LogInformation("Sending email to {To} with subject: {Subject}", to, subject);
        Thread.Sleep(2000); // Simulate processing
        _logger.LogInformation("Email sent successfully to {To}", to);
    }

    public void SendDelayedEmail(string to, string subject, string body)
    {
        _logger.LogInformation("Sending delayed email to {To} with subject: {Subject}", to, subject);
        Thread.Sleep(2000);
        _logger.LogInformation("Delayed email sent successfully to {To}", to);
    }
}
