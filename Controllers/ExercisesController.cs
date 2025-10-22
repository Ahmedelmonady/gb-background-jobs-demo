using Hangfire;
using HangfireDemo.Models;
using HangfireDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace HangfireDemo.Controllers;

/// <summary>
/// Starter code for hands-on exercises
/// Participants will complete the TODO sections
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ExercisesController : ControllerBase
{
    private readonly ILogger<ExercisesController> _logger;

    public ExercisesController(ILogger<ExercisesController> logger)
    {
        _logger = logger;
    }

    // ==========================================
    // EXERCISE 1: Todo Reminder System
    // ==========================================

    /// <summary>
    /// Exercise 1: Create a todo with reminder
    /// TODO: Schedule a background job to send reminder at the specified time
    /// </summary>
    [HttpPost("exercise1/todo")]
    public IActionResult CreateTodoWithReminder([FromBody] TodoRequest request)
    {
        _logger.LogInformation("Creating todo: {Title}", request.Title);

        // TODO: Schedule a job to send reminder at request.RemindAt
        // Hint: Use BackgroundJob.Schedule<ITodoService>(...)

        // SOLUTION (uncomment):
        // var jobId = BackgroundJob.Schedule<ITodoService>(
        //     service => service.SendReminder(request.Id, request.Title),
        //     request.RemindAt);
        //
        // return Ok(new
        // {
        //     message = "Todo created with reminder",
        //     todoId = request.Id,
        //     remindAt = request.RemindAt,
        //     jobId = jobId
        // });

        return Ok(new
        {
            message = "TODO: Implement job scheduling",
            todoId = request.Id,
            remindAt = request.RemindAt
        });
    }

    /// <summary>
    /// Exercise 1 Bonus: Recurring reminders every hour
    /// TODO: Create a recurring job that reminds every hour
    /// </summary>
    [HttpPost("exercise1/recurring-reminder")]
    public IActionResult CreateRecurringReminder([FromBody] TodoRequest request)
    {
        _logger.LogInformation("Creating recurring reminder for todo: {Title}", request.Title);

        // TODO: Create a recurring job that runs hourly
        // Hint: Use RecurringJob.AddOrUpdate with Cron.Hourly()

        // SOLUTION (uncomment):
        // var jobId = $"todo-reminder-{request.Id}";
        // RecurringJob.AddOrUpdate<ITodoService>(
        //     jobId,
        //     service => service.SendReminder(request.Id, request.Title),
        //     Cron.Hourly());
        //
        // return Ok(new
        // {
        //     message = "Recurring reminder created (every hour)",
        //     todoId = request.Id,
        //     jobId = jobId,
        //     schedule = "Hourly"
        // });

        return Ok(new { message = "TODO: Implement recurring job" });
    }

    // ==========================================
    // EXERCISE 2: Report Generation Service
    // ==========================================

    /// <summary>
    /// Exercise 2: Generate report and send email
    /// TODO: Use fire-and-forget for report generation, then continuation for email
    /// </summary>
    [HttpPost("exercise2/generate-report")]
    public IActionResult GenerateReportAsync([FromBody] ReportRequest request)
    {
        _logger.LogInformation("Requesting report generation for Report ID: {ReportId}", request.ReportId);

        // TODO: Step 1 - Enqueue report generation job (fire-and-forget)
        // TODO: Step 2 - Chain email notification job (continuation)
        // Hint: Use BackgroundJob.Enqueue and BackgroundJob.ContinueJobWith

        // SOLUTION (uncomment):
        // var reportJobId = BackgroundJob.Enqueue<IReportService>(
        //     service => service.GenerateReport(request.ReportId));
        //
        // var emailJobId = BackgroundJob.ContinueJobWith<IReportService>(
        //     reportJobId,
        //     service => service.SendReportNotification(
        //         $"report_{request.ReportId}.csv",
        //         request.Email));
        //
        // return Accepted(new
        // {
        //     message = "Report generation started",
        //     reportJobId = reportJobId,
        //     emailJobId = emailJobId,
        //     dashboardUrl = "/hangfire"
        // });

        return Accepted(new { message = "TODO: Implement report generation with continuation" });
    }

    // ==========================================
    // EXERCISE 3: Data Cleanup Job
    // ==========================================

    /// <summary>
    /// Exercise 3: Create daily cleanup job
    /// TODO: Schedule a recurring job to clean old logs daily at 3 AM
    /// </summary>
    [HttpPost("exercise3/start-cleanup")]
    public IActionResult StartDailyCleanup()
    {
        _logger.LogInformation("Starting daily cleanup job");

        // TODO: Create a recurring job that runs daily at 3 AM
        // Hint: Use RecurringJob.AddOrUpdate with Cron.Daily(3, 0)

        // SOLUTION (uncomment):
        // RecurringJob.AddOrUpdate<ICleanupService>(
        //     "daily-log-cleanup",
        //     service => service.CleanupOldLogs(),
        //     Cron.Daily(3, 0));  // 3:00 AM daily
        //
        // return Ok(new
        // {
        //     message = "Daily cleanup job created",
        //     jobId = "daily-log-cleanup",
        //     schedule = "Daily at 3:00 AM",
        //     nextRun = DateTime.Today.AddDays(1).AddHours(3),
        //     dashboardUrl = "/hangfire"
        // });

        return Ok(new { message = "TODO: Implement daily cleanup job" });
    }

    /// <summary>
    /// Exercise 3: Trigger cleanup manually
    /// TODO: Manually trigger the cleanup job (useful for testing)
    /// </summary>
    [HttpPost("exercise3/trigger-now")]
    public IActionResult TriggerCleanupNow()
    {
        _logger.LogInformation("Manually triggering cleanup job");

        // TODO: Trigger the recurring job immediately
        // Hint: Use RecurringJob.Trigger("job-id")

        // SOLUTION (uncomment):
        // RecurringJob.Trigger("daily-log-cleanup");
        //
        // return Ok(new
        // {
        //     message = "Cleanup job triggered manually",
        //     dashboardUrl = "/hangfire"
        // });

        return Ok(new { message = "TODO: Implement manual trigger" });
    }

    /// <summary>
    /// Exercise 3: Stop cleanup job
    /// </summary>
    [HttpPost("exercise3/stop-cleanup")]
    public IActionResult StopDailyCleanup()
    {
        _logger.LogInformation("Stopping daily cleanup job");

        // TODO: Remove the recurring job
        // Hint: Use RecurringJob.RemoveIfExists("job-id")

        // SOLUTION (uncomment):
        // RecurringJob.RemoveIfExists("daily-log-cleanup");
        //
        // return Ok(new
        // {
        //     message = "Daily cleanup job stopped",
        //     jobId = "daily-log-cleanup"
        // });

        return Ok(new { message = "TODO: Implement job removal" });
    }
}
