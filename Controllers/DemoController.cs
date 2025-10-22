using Hangfire;
using Hangfire.States;
using HangfireDemo.Models;
using HangfireDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace HangfireDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DemoController : ControllerBase
{
    private readonly ILogger<DemoController> _logger;

    public DemoController(ILogger<DemoController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Demo 1: Fire-and-Forget Job
    /// Sends an email immediately in the background
    /// </summary>
    [HttpPost("fire-and-forget")]
    public IActionResult FireAndForget([FromBody] EmailRequest request)
    {
        _logger.LogInformation("Enqueueing fire-and-forget email job");

        var jobId = BackgroundJob.Enqueue<IEmailService>(
            service => service.SendEmail(request.To, request.Subject, request.Body));

        return Ok(new
        {
            message = "Email job enqueued (fire-and-forget)",
            jobId = jobId,
            dashboardUrl = "/hangfire"
        });
    }

    /// <summary>
    /// Demo 2: Delayed Job
    /// Schedules an email to be sent after a delay
    /// </summary>
    [HttpPost("delayed")]
    public IActionResult DelayedJob([FromBody] EmailRequest request)
    {
        var delayMinutes = request.DelayMinutes ?? 2;
        _logger.LogInformation("Scheduling delayed email job for {DelayMinutes} minutes", delayMinutes);

        var jobId = BackgroundJob.Schedule<IEmailService>(
            service => service.SendDelayedEmail(request.To, request.Subject, request.Body),
            TimeSpan.FromMinutes(delayMinutes));

        return Ok(new
        {
            message = $"Email scheduled to be sent in {delayMinutes} minutes",
            jobId = jobId,
            scheduledFor = DateTime.Now.AddMinutes(delayMinutes),
            dashboardUrl = "/hangfire"
        });
    }

    /// <summary>
    /// Demo 3: Recurring Job
    /// Creates a recurring job that runs every minute
    /// </summary>
    [HttpPost("recurring/start")]
    public IActionResult StartRecurringJob()
    {
        _logger.LogInformation("Creating recurring cleanup job");

        RecurringJob.AddOrUpdate<ICleanupService>(
            "cleanup-sessions",
            service => service.CleanupExpiredSessions(),
            Cron.Minutely());  // Runs every minute

        return Ok(new
        {
            message = "Recurring job created (runs every minute)",
            jobId = "cleanup-sessions",
            schedule = "Every minute",
            dashboardUrl = "/hangfire"
        });
    }

    /// <summary>
    /// Stop the recurring job
    /// </summary>
    [HttpPost("recurring/stop")]
    public IActionResult StopRecurringJob()
    {
        _logger.LogInformation("Stopping recurring cleanup job");

        RecurringJob.RemoveIfExists("cleanup-sessions");

        return Ok(new
        {
            message = "Recurring job stopped",
            jobId = "cleanup-sessions"
        });
    }

    /// <summary>
    /// Demo 4: Job Continuation (Chaining)
    /// Generates a report, then sends an email notification
    /// </summary>
    [HttpPost("continuation")]
    public IActionResult ContinuationJob([FromBody] ReportRequest request)
    {
        _logger.LogInformation("Creating job chain: Generate Report -> Send Email");

        // Step 1: Generate report
        var reportJobId = BackgroundJob.Enqueue<IReportService>(
            service => service.GenerateReport(request.ReportId));

        // Step 2: After report is generated, send notification
        var emailJobId = BackgroundJob.ContinueJobWith<IReportService>(
            reportJobId,
            service => service.SendReportNotification(
                $"report_{request.ReportId}.csv",
                request.Email));

        return Ok(new
        {
            message = "Job chain created: Report Generation -> Email Notification",
            reportJobId = reportJobId,
            emailJobId = emailJobId,
            dashboardUrl = "/hangfire"
        });
    }

    /// <summary>
    /// Demo 5: Complex Job Chain (3 steps)
    /// Import -> Validate -> Send Notification
    /// </summary>
    [HttpPost("complex-chain")]
    public IActionResult ComplexChain()
    {
        _logger.LogInformation("Creating complex job chain");

        var job1 = BackgroundJob.Enqueue(() => Console.WriteLine("Step 1: Importing data..."));
        var job2 = BackgroundJob.ContinueJobWith(job1, () => Console.WriteLine("Step 2: Validating data..."));
        var job3 = BackgroundJob.ContinueJobWith(job2, () => Console.WriteLine("Step 3: Sending notification..."));

        return Ok(new
        {
            message = "Complex job chain created (3 steps)",
            step1JobId = job1,
            step2JobId = job2,
            step3JobId = job3,
            dashboardUrl = "/hangfire"
        });
    }

    /// <summary>
    /// Demo 6: Queue Priority
    /// Submit jobs to different queues (critical vs default)
    /// </summary>
    [HttpPost("queue-priority")]
    public IActionResult QueuePriority()
    {
        _logger.LogInformation("Enqueueing jobs to different queues");

        var client = new BackgroundJobClient();

        // Normal priority (default queue)
        var normalJobId = BackgroundJob.Enqueue(() =>
            Console.WriteLine("Normal priority job executing..."));

        // Critical priority (critical queue - processed first)
        var criticalJobId = client.Create(() =>
            Console.WriteLine("CRITICAL priority job executing..."),
            new EnqueuedState("critical"));

        return Ok(new
        {
            message = "Jobs enqueued to different queues",
            normalJobId = normalJobId,
            normalQueue = "default",
            criticalJobId = criticalJobId,
            criticalQueue = "critical",
            dashboardUrl = "/hangfire"
        });
    }    

    /// <summary>
    /// Demo 7: Delete/Cancel a job
    /// </summary>
    [HttpDelete("cancel/{jobId}")]
    public IActionResult CancelJob(string jobId)
    {
        _logger.LogInformation("Cancelling job {JobId}", jobId);

        var deleted = BackgroundJob.Delete(jobId);

        return Ok(new
        {
            message = deleted ? "Job cancelled successfully" : "Job not found or already processed",
            jobId = jobId,
            deleted = deleted
        });
    }
}
