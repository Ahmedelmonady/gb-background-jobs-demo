# Hands-On Exercise Solutions

Complete solutions for all three exercises.

---

## Exercise 1: Todo Reminder System

### Part A: Single Reminder

**Task**: Schedule a job to send a reminder at a specific time.

**Solution**:
```csharp
[HttpPost("exercise1/todo")]
public IActionResult CreateTodoWithReminder([FromBody] TodoRequest request)
{
    _logger.LogInformation("Creating todo: {Title}", request.Title);

    // Schedule the reminder job
    var jobId = BackgroundJob.Schedule<ITodoService>(
        service => service.SendReminder(request.Id, request.Title),
        request.RemindAt);

    return Ok(new
    {
        message = "Todo created with reminder",
        todoId = request.Id,
        title = request.Title,
        remindAt = request.RemindAt,
        jobId = jobId,
        dashboardUrl = "/hangfire"
    });
}
```

**Explanation**:
1. Use `BackgroundJob.Schedule<ITodoService>` to schedule a delayed job
2. Pass the service method with parameters: `service.SendReminder(request.Id, request.Title)`
3. Specify when to run: `request.RemindAt`
4. Return the `jobId` so it can be tracked or cancelled

**Test**:
```bash
curl -X POST https://localhost:7000/api/exercises/exercise1/todo \
  -H "Content-Type: application/json" \
  -d '{
    "id": 1,
    "title": "Complete Hangfire presentation",
    "remindAt": "2024-12-31T15:00:00"
  }'
```

---

### Part B: Recurring Reminders (Bonus)

**Task**: Create a recurring reminder that runs every hour.

**Solution**:
```csharp
[HttpPost("exercise1/recurring-reminder")]
public IActionResult CreateRecurringReminder([FromBody] TodoRequest request)
{
    _logger.LogInformation("Creating recurring reminder for todo: {Title}", request.Title);

    // Create unique job ID for this todo
    var jobId = $"todo-reminder-{request.Id}";

    // Create recurring job
    RecurringJob.AddOrUpdate<ITodoService>(
        jobId,
        service => service.SendReminder(request.Id, request.Title),
        Cron.Hourly());

    return Ok(new
    {
        message = "Recurring reminder created (every hour)",
        todoId = request.Id,
        title = request.Title,
        jobId = jobId,
        schedule = "Hourly",
        dashboardUrl = "/hangfire"
    });
}
```

**Explanation**:
1. Create a unique job ID: `$"todo-reminder-{request.Id}"`
2. Use `RecurringJob.AddOrUpdate` to create/update the recurring job
3. Use `Cron.Hourly()` for hourly execution
4. The job will run every hour until explicitly removed

**Stop the recurring reminder**:
```csharp
[HttpDelete("exercise1/recurring-reminder/{todoId}")]
public IActionResult StopRecurringReminder(int todoId)
{
    var jobId = $"todo-reminder-{todoId}";
    RecurringJob.RemoveIfExists(jobId);

    return Ok(new { message = "Recurring reminder stopped", jobId = jobId });
}
```

**Test**:
```bash
# Start recurring reminder
curl -X POST https://localhost:7000/api/exercises/exercise1/recurring-reminder \
  -H "Content-Type: application/json" \
  -d '{"id": 1, "title": "Review PRs", "remindAt": "2024-01-01T00:00:00"}'

# Check dashboard - you'll see it in "Recurring Jobs" tab
# Trigger manually for immediate test
```

---

## Exercise 2: Report Generation Service

### Task: Generate report and send email notification

**Solution**:
```csharp
[HttpPost("exercise2/generate-report")]
public IActionResult GenerateReportAsync([FromBody] ReportRequest request)
{
    _logger.LogInformation("Requesting report generation for Report ID: {ReportId}", request.ReportId);

    // Step 1: Enqueue report generation (fire-and-forget)
    var reportJobId = BackgroundJob.Enqueue<IReportService>(
        service => service.GenerateReport(request.ReportId));

    // Step 2: Chain email notification (continuation)
    var emailJobId = BackgroundJob.ContinueJobWith<IReportService>(
        reportJobId,
        service => service.SendReportNotification(
            $"report_{request.ReportId}.csv",
            request.Email));

    return Accepted(new
    {
        message = "Report generation started. Email will be sent when complete.",
        reportJobId = reportJobId,
        emailJobId = emailJobId,
        reportId = request.ReportId,
        email = request.Email,
        dashboardUrl = "/hangfire"
    });
}
```

**Explanation**:
1. **Fire-and-forget job**: `BackgroundJob.Enqueue` starts report generation immediately
2. **Continuation job**: `BackgroundJob.ContinueJobWith` chains the email job
3. The email job only runs after the report job succeeds
4. Return `Accepted` (202) since processing happens asynchronously

**Flow**:
```
Request → Enqueue Report Job → Wait 5 seconds → Complete
                                      ↓
                            Email Job Starts → Wait 1 second → Complete
```

**Test**:
```bash
curl -X POST https://localhost:7000/api/exercises/exercise2/generate-report \
  -H "Content-Type: application/json" \
  -d '{
    "reportId": 123,
    "email": "admin@gameball.co"
  }'
```

**View in dashboard**:
1. Go to `/hangfire`
2. Click on Jobs → Processing
3. You'll see the report job running (5 seconds)
4. When it completes, the email job automatically starts (1 second)

---

### Bonus: Add Progress Tracking

**Enhanced solution with job state**:
```csharp
[HttpPost("exercise2/generate-report-advanced")]
public IActionResult GenerateReportWithProgress([FromBody] ReportRequest request)
{
    var reportJobId = BackgroundJob.Enqueue<IReportService>(
        service => service.GenerateReport(request.ReportId));

    // Store job ID for tracking
    // In real app, save to database with reportId

    var emailJobId = BackgroundJob.ContinueJobWith<IReportService>(
        reportJobId,
        service => service.SendReportNotification(
            $"report_{request.ReportId}.csv",
            request.Email));

    return Accepted(new
    {
        message = "Report generation started",
        reportJobId = reportJobId,
        emailJobId = emailJobId,
        statusCheckUrl = $"/api/exercises/exercise2/status/{reportJobId}"
    });
}

[HttpGet("exercise2/status/{jobId}")]
public IActionResult GetJobStatus(string jobId)
{
    var monitor = JobStorage.Current.GetMonitoringApi();
    var jobDetails = monitor.JobDetails(jobId);

    if (jobDetails == null)
        return NotFound(new { message = "Job not found" });

    return Ok(new
    {
        jobId = jobId,
        state = jobDetails.History[0].StateName,
        createdAt = jobDetails.CreatedAt,
        properties = jobDetails.Properties
    });
}
```

---

## Exercise 3: Data Cleanup Job

### Part A: Start Daily Cleanup

**Task**: Create a recurring job that runs daily at 3 AM.

**Solution**:
```csharp
[HttpPost("exercise3/start-cleanup")]
public IActionResult StartDailyCleanup()
{
    _logger.LogInformation("Starting daily cleanup job");

    // Create recurring job with cron expression
    RecurringJob.AddOrUpdate<ICleanupService>(
        "daily-log-cleanup",
        service => service.CleanupOldLogs(),
        Cron.Daily(3, 0));  // 3:00 AM every day

    // Calculate next run time
    var nextRun = DateTime.Today.AddDays(1).AddHours(3);
    if (DateTime.Now.Hour < 3)
        nextRun = DateTime.Today.AddHours(3);

    return Ok(new
    {
        message = "Daily cleanup job created successfully",
        jobId = "daily-log-cleanup",
        schedule = "Daily at 3:00 AM UTC",
        nextRun = nextRun,
        dashboardUrl = "/hangfire",
        tip = "Use POST /api/exercises/exercise3/trigger-now to test immediately"
    });
}
```

**Explanation**:
1. Use `RecurringJob.AddOrUpdate` to create the job
2. Job ID: `"daily-log-cleanup"` (unique identifier)
3. Schedule: `Cron.Daily(3, 0)` means 3:00 AM every day
4. If job already exists with same ID, it will be updated

---

### Part B: Trigger Manually

**Task**: Allow manual triggering for testing.

**Solution**:
```csharp
[HttpPost("exercise3/trigger-now")]
public IActionResult TriggerCleanupNow()
{
    _logger.LogInformation("Manually triggering cleanup job");

    // Trigger the recurring job immediately
    RecurringJob.Trigger("daily-log-cleanup");

    return Ok(new
    {
        message = "Cleanup job triggered successfully",
        jobId = "daily-log-cleanup",
        status = "Processing",
        dashboardUrl = "/hangfire",
        tip = "Check the dashboard to see job execution"
    });
}
```

**Explanation**:
- `RecurringJob.Trigger("daily-log-cleanup")` executes the job immediately
- Useful for testing without waiting for scheduled time
- The job will still run on its regular schedule

---

### Part C: Stop Cleanup

**Task**: Remove the recurring job.

**Solution**:
```csharp
[HttpPost("exercise3/stop-cleanup")]
public IActionResult StopDailyCleanup()
{
    _logger.LogInformation("Stopping daily cleanup job");

    // Remove the recurring job
    RecurringJob.RemoveIfExists("daily-log-cleanup");

    return Ok(new
    {
        message = "Daily cleanup job stopped and removed",
        jobId = "daily-log-cleanup",
        status = "Removed"
    });
}
```

**Explanation**:
- `RecurringJob.RemoveIfExists` safely removes the job
- If job doesn't exist, no error is thrown
- Job won't run again unless recreated

---

### Bonus: Add Error Handling and Retry

**Enhanced cleanup service with retry**:
```csharp
public class CleanupService : ICleanupService
{
    private readonly ILogger<CleanupService> _logger;

    public CleanupService(ILogger<CleanupService> logger)
    {
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public void CleanupOldLogs()
    {
        try
        {
            var cutoffDate = DateTime.Now.AddDays(-7);

            Log.ForContext("CutoffDate", cutoffDate)
               .ForContext("JobType", "CleanupOldLogs")
               .Information("Starting cleanup of logs older than {CutoffDate}", cutoffDate);

            // Simulate cleanup operation
            Thread.Sleep(3000);

            // Simulate potential error (for testing retry)
            // throw new Exception("Database connection failed");

            var deletedCount = new Random().Next(10, 100);

            Log.ForContext("DeletedCount", deletedCount)
               .ForContext("JobType", "CleanupOldLogs")
               .Information("Cleanup completed successfully. Deleted {DeletedCount} log entries", deletedCount);
        }
        catch (Exception ex)
        {
            Log.ForContext("JobType", "CleanupOldLogs")
               .Error(ex, "Error during log cleanup");
            throw; // Re-throw to trigger Hangfire retry
        }
    }
}
```

**Features**:
1. `[AutomaticRetry(Attempts = 3)]` - Retry up to 3 times if it fails
2. Structured logging with context
3. Proper exception handling
4. Re-throw exception to trigger retry

---

## Complete Test Flow

### Exercise 1 Test:
```bash
# Create todo with reminder in 2 minutes
curl -X POST https://localhost:7000/api/exercises/exercise1/todo \
  -H "Content-Type: application/json" \
  -d "{
    \"id\": 1,
    \"title\": \"Test Hangfire\",
    \"remindAt\": \"$(date -u -d '+2 minutes' '+%Y-%m-%dT%H:%M:%S')\"
  }"

# Wait 2 minutes and check logs
```

### Exercise 2 Test:
```bash
# Generate report
curl -X POST https://localhost:7000/api/exercises/exercise2/generate-report \
  -H "Content-Type: application/json" \
  -d '{
    "reportId": 999,
    "email": "test@example.com"
  }'

# Go to dashboard immediately
# Watch: Report job (5 sec) → Email job (1 sec)
```

### Exercise 3 Test:
```bash
# Start daily cleanup
curl -X POST https://localhost:7000/api/exercises/exercise3/start-cleanup

# Trigger immediately (for testing)
curl -X POST https://localhost:7000/api/exercises/exercise3/trigger-now

# Check dashboard - see job execution

# Stop cleanup
curl -X POST https://localhost:7000/api/exercises/exercise3/stop-cleanup
```

---

## Key Takeaways

### Exercise 1 (Delayed Jobs)
- ✅ Use `BackgroundJob.Schedule` for delayed execution
- ✅ Pass DateTime to specify exact run time
- ✅ Use `RecurringJob` for repeating reminders

### Exercise 2 (Continuations)
- ✅ Use `BackgroundJob.Enqueue` for immediate jobs
- ✅ Chain jobs with `BackgroundJob.ContinueJobWith`
- ✅ Continuations only run if parent job succeeds
- ✅ Return `Accepted` (202) for async operations

### Exercise 3 (Recurring Jobs)
- ✅ Use `RecurringJob.AddOrUpdate` for scheduled jobs
- ✅ Use cron expressions for scheduling
- ✅ `RecurringJob.Trigger` for manual execution
- ✅ `RecurringJob.RemoveIfExists` to stop jobs
- ✅ Add `[AutomaticRetry]` for resilience

---

## Common Mistakes to Avoid

### ❌ Mistake 1: Passing complex objects
```csharp
// Bad
BackgroundJob.Enqueue(() => Process(dbContext));

// Good
BackgroundJob.Enqueue(() => Process(entityId));
```

### ❌ Mistake 2: Not using dependency injection
```csharp
// Bad
BackgroundJob.Enqueue(() => new MyService().DoWork());

// Good
BackgroundJob.Enqueue<IMyService>(service => service.DoWork());
```

### ❌ Mistake 3: Forgetting to handle errors
```csharp
// Bad
public void ProcessData()
{
    // No error handling - job fails silently
}

// Good
[AutomaticRetry(Attempts = 3)]
public void ProcessData()
{
    try
    {
        // Logic
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error in ProcessData");
        throw; // Let Hangfire retry
    }
}
```

### ❌ Mistake 4: Not using unique job IDs for recurring jobs
```csharp
// Bad - hard to manage multiple similar jobs
RecurringJob.AddOrUpdate("cleanup", () => Cleanup(clientId), Cron.Daily());

// Good - unique per client
RecurringJob.AddOrUpdate($"cleanup-{clientId}", () => Cleanup(clientId), Cron.Daily());
```

---

**Congratulations! You've completed all exercises! 🎉**

Next steps:
1. Review Gameball production examples
2. Implement a background job in a real feature
3. Explore the Hangfire dashboard thoroughly
4. Read the Hangfire documentation
